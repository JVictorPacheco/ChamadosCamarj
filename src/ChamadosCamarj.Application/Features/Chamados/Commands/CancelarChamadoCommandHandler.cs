using MediatR;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Application.Common.Extensions;
using ChamadosCamarj.Application.Common.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class CancelarChamadoCommandHandler : IRequestHandler<CancelarChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IHistoricoRepository _historicoRepository;
    private readonly IPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;

    public CancelarChamadoCommandHandler(IChamadoRepository chamadoRepository, IHistoricoRepository historicoRepository, IPublisher publisher, IUnitOfWork unitOfWork)
    {
        _chamadoRepository = chamadoRepository;
        _historicoRepository = historicoRepository;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelarChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        chamado.Cancelar(request.Motivo, request.MotivoOutro);

        await using var _ = _unitOfWork;
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);

        var motivoLabel = request.Motivo == MotivoEncerramento.Outro && !string.IsNullOrWhiteSpace(request.MotivoOutro)
            ? $"{request.Motivo}: {request.MotivoOutro}"
            : request.Motivo.ToString();

        await _historicoRepository.RegistrarHistoricoAsync(
            chamado.Id,
            AcaoHistorico.Cancelado,
            detalheNovo: motivoLabel,
            usuarioNome: request.UsuarioNome,
            usuarioId: request.UsuarioId,
            cancellationToken: cancellationToken
        );

        var textoComentario = $"Chamado cancelado. Motivo: {motivoLabel}.";
        if (!string.IsNullOrWhiteSpace(request.Observacao))
            textoComentario += $" Observação: {request.Observacao.Trim()}";

        var comentario = new Comentario(
            request.Id,
            request.UsuarioNome,
            textoComentario,
            TipoComentario.Publico
        );
        await _chamadoRepository.AdicionarComentarioAsync(comentario, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        await _publisher.Publish(new StatusAlteradoNotification(
            chamado.Id,
            chamado.Status.ToString(),
            chamado.DataAtualizacao ?? DateTime.UtcNow
        ), cancellationToken);
    }
}
