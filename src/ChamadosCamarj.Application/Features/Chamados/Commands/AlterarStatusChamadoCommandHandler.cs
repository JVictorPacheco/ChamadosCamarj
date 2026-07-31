using MediatR;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Extensions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class AlterarStatusChamadoCommandHandler : IRequestHandler<AlterarStatusChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IHistoricoRepository _historicoRepository;
    private readonly IPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;

    public AlterarStatusChamadoCommandHandler(
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

    public async Task Handle(AlterarStatusChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdComTrackingAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        var statusAnterior = chamado.Status;

        chamado.AlterarStatus(request.NovoStatus);

        await using var _ = _unitOfWork;
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);

        await _historicoRepository.RegistrarHistoricoAsync(
            chamado.Id,
            AcaoHistorico.StatusAlterado,
            detalheAnterior: statusAnterior.ToString(),
            detalheNovo: chamado.Status.ToString(),
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
