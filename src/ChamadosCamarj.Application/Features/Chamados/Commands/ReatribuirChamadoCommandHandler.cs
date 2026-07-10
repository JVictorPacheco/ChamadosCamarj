using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class ReatribuirChamadoCommandHandler : IRequestHandler<ReatribuirChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IHistoricoRepository _historicoRepository;
    private readonly IPublisher _publisher;

    public ReatribuirChamadoCommandHandler(
        IChamadoRepository chamadoRepository,
        IHistoricoRepository historicoRepository,
        IPublisher publisher)
    {
        _chamadoRepository = chamadoRepository;
        _historicoRepository = historicoRepository;
        _publisher = publisher;
    }

    public async Task Handle(ReatribuirChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        var responsavelAnterior = chamado.ResponsavelNome ?? "Não atribuído";
        
        chamado.Reatribuir(request.NovoResponsavelId, request.NovoResponsavelNome);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);

        // Gerar entrada no histórico
        var historico = HistoricoEntrada.Criar(
            chamado.Id,
            "Sistema", // Será sobrescrito com usuário real depois (T05)
            null,
            AcaoHistorico.Reatribuido,
            detalheAnterior: responsavelAnterior,
            detalheNovo: request.NovoResponsavelNome
        );
        await _historicoRepository.AdicionarAsync(historico, cancellationToken);

        await _publisher.Publish(new StatusAlteradoNotification(
            chamado.Id,
            chamado.Status.ToString(),
            chamado.DataAtualizacao ?? DateTime.UtcNow
        ), cancellationToken);
    }
}
