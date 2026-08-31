using MediatR;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Commands.AtualizarPresenca;

public class AtualizarPresencaCommandHandler : IRequestHandler<AtualizarPresencaCommand>
{
    private readonly IChatPresencaRepository _presencaRepository;
    private readonly IMediator _mediator;

    public AtualizarPresencaCommandHandler(IChatPresencaRepository presencaRepository, IMediator mediator)
    {
        _presencaRepository = presencaRepository;
        _mediator = mediator;
    }

    public async Task Handle(AtualizarPresencaCommand request, CancellationToken cancellationToken)
    {
        var presenca = await _presencaRepository.ObterPorUsuarioAsync(request.UsuarioId, cancellationToken)
            ?? new ChatPresenca(request.UsuarioId, request.UsuarioNome);

        if (request.Status is null)
            presenca.AtualizarHeartbeat();
        else
            presenca.DefinirStatus(request.Status.Value);

        await _presencaRepository.AdicionarOuAtualizarAsync(presenca, cancellationToken);

        await _mediator.Publish(
            new ChatPresencaAtualizadaNotification(presenca.UsuarioId, presenca.UsuarioNome, presenca.Status.ToString()),
            cancellationToken);
    }
}
