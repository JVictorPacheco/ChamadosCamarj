using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Common.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class ForcarEncerramentoChamadoCommandHandler : IRequestHandler<ForcarEncerramentoChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IHistoricoRepository _historicoRepository;
    private readonly IPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;

    public ForcarEncerramentoChamadoCommandHandler(
        IChamadoRepository chamadoRepository,
        IHistoricoRepository historicoRepository,
        IPublisher publisher,
        IUnitOfWork unitOfWork)
    {
        _chamadoRepository = chamadoRepository;
        _historicoRepository = historicoRepository;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ForcarEncerramentoChamadoCommand request, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var chamado = await _chamadoRepository.ObterPorIdComTrackingAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        var statusAnterior = chamado.Status;

        chamado.ForcarEncerramento(request.Motivo, request.MotivoOutro);

        await using var _ = _unitOfWork;
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);

        var motivoLabel = request.Motivo == MotivoEncerramento.Outro && !string.IsNullOrWhiteSpace(request.MotivoOutro)
            ? $"{request.Motivo}: {request.MotivoOutro}"
            : request.Motivo.ToString();

        var historico = HistoricoEntrada.Criar(
            chamado.Id,
            request.UsuarioNome,
            request.UsuarioId,
            AcaoHistorico.EncerramentoForcado,
            detalheAnterior: statusAnterior.ToString(),
            detalheNovo: motivoLabel
        );
        await _historicoRepository.AdicionarAsync(historico, cancellationToken);

        var textoComentario = $"Chamado encerrado forçadamente. Motivo: {motivoLabel}.";
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
