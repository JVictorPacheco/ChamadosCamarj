using System.Text.Json;
using MediatR;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Extensions;
using ChamadosCamarj.Application.Common.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class AtualizarChamadoCommandHandler : IRequestHandler<AtualizarChamadoCommand>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly IChamadoRepository _chamadoRepository;
    private readonly IHistoricoRepository _historicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarChamadoCommandHandler(
        IChamadoRepository chamadoRepository,
        IHistoricoRepository historicoRepository,
        IUnitOfWork unitOfWork)
    {
        _chamadoRepository = chamadoRepository;
        _historicoRepository = historicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AtualizarChamadoCommand request, CancellationToken cancellationToken)
    {
        // Com tracking, como os demais handlers de escrita desde o controle de concorrência (2026-07-31):
        // sem ele, o EF compara DataAtualizacao com o valor já alterado e todo save dava 409.
        var chamado = await _chamadoRepository.ObterPorIdComTrackingAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        var tituloAnterior = chamado.Titulo;
        var descricaoAnterior = chamado.Descricao;

        bool mudou;
        try
        {
            mudou = chamado.AtualizarDados(request.Titulo, request.Descricao);
        }
        catch (InvalidOperationException)
        {
            // Encerrado entre a checagem do behaviour e aqui: mesma mensagem do AC-10, não erro 500.
            throw new BadRequestException(ChamadoPermissoes.MensagemEdicaoEncerrado);
        }

        // Nada mudou: não grava nem registra (spec editar-chamado AC-16).
        if (!mudou)
            return;

        // Histórico com antes e depois só do que mudou (AC-17/AC-18), em JSON por campo.
        var anterior = new Dictionary<string, string>();
        var novo = new Dictionary<string, string>();
        if (tituloAnterior != chamado.Titulo) { anterior["titulo"] = tituloAnterior; novo["titulo"] = chamado.Titulo; }
        if (descricaoAnterior != chamado.Descricao) { anterior["descricao"] = descricaoAnterior; novo["descricao"] = chamado.Descricao; }

        await using var _ = _unitOfWork;
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);

        await _historicoRepository.RegistrarHistoricoAsync(
            chamado.Id,
            AcaoHistorico.ChamadoEditado,
            detalheAnterior: JsonSerializer.Serialize(anterior, Json),
            detalheNovo: JsonSerializer.Serialize(novo, Json),
            usuarioNome: request.UsuarioNome,
            usuarioId: request.UsuarioId,
            cancellationToken: cancellationToken
        );

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
