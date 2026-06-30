using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class AtribuirChamadoCommandHandler : IRequestHandler<AtribuirChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IPublisher _publisher;

    public AtribuirChamadoCommandHandler(IChamadoRepository chamadoRepository, IPublisher publisher)
    {
        _chamadoRepository = chamadoRepository;
        _publisher = publisher;
    }

    public async Task Handle(AtribuirChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        chamado.Atribuir(request.ResponsavelId, request.ResponsavelNome);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);

        await _publisher.Publish(new StatusAlteradoNotification(
            chamado.Id,
            chamado.Status.ToString(),
            chamado.DataAtualizacao ?? DateTime.UtcNow
        ), cancellationToken);
    }
}
