using MediatR;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Application.Common.Extensions;
using ChamadosCamarj.Application.Common.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class FecharChamadoCommandHandler : IRequestHandler<FecharChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IHistoricoRepository _historicoRepository;
    private readonly IPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;

    public FecharChamadoCommandHandler(IChamadoRepository chamadoRepository, IHistoricoRepository historicoRepository, IPublisher publisher, IUnitOfWork unitOfWork)
    {
        _chamadoRepository = chamadoRepository;
        _historicoRepository = historicoRepository;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(FecharChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        chamado.Fechar(request.Motivo, request.MotivoOutro);

        await using var _ = _unitOfWork;
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);

        var motivoLabel = request.Motivo == MotivoEncerramento.Outro && !string.IsNullOrWhiteSpace(request.MotivoOutro)
            ? $"{request.Motivo}: {request.MotivoOutro}"
            : request.Motivo.ToString();

        var textoComentario = $"Chamado encerrado. Motivo: {motivoLabel}.";
        if (!string.IsNullOrWhiteSpace(request.Observacao))
            textoComentario += $" Observação: {request.Observacao.Trim()}";

        var comentario = new Comentario(
            request.Id,
            request.UsuarioNome,
            textoComentario,
            TipoComentario.Publico
        );
        await _chamadoRepository.AdicionarComentarioAsync(comentario, cancellationToken);

        await _historicoRepository.RegistrarHistoricoAsync(
            chamado.Id,
            AcaoHistorico.Fechado,
            detalheNovo: motivoLabel,
            usuarioNome: request.UsuarioNome,
            usuarioId: request.UsuarioId,
            cancellationToken: cancellationToken
        );

        await _unitOfWork.CommitAsync(cancellationToken);

        await _publisher.Publish(new StatusAlteradoNotification(
            chamado.Id,
            chamado.Status.ToString(),
            chamado.DataAtualizacao ?? DateTime.UtcNow
        ), cancellationToken);
    }
}
