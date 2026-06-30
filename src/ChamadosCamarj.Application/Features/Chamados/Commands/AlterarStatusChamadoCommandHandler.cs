using MediatR;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class AlterarStatusChamadoCommandHandler : IRequestHandler<AlterarStatusChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IPublisher _publisher;

    public AlterarStatusChamadoCommandHandler(IChamadoRepository chamadoRepository, IPublisher publisher)
    {
        _chamadoRepository = chamadoRepository;
        _publisher = publisher;
    }

    public async Task Handle(AlterarStatusChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        chamado.AlterarStatus(request.NovoStatus);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);

        await _publisher.Publish(new StatusAlteradoNotification(
            chamado.Id,
            chamado.Status.ToString(),
            chamado.DataAtualizacao ?? DateTime.UtcNow
        ), cancellationToken);
    }
}
