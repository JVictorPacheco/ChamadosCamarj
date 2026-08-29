using MediatR;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Commands.MarcarComoLido;

public class MarcarComoLidoCommandHandler : IRequestHandler<MarcarComoLidoCommand>
{
    private readonly IChatConversaRepository _conversaRepository;
    private readonly IMediator _mediator;

    public MarcarComoLidoCommandHandler(IChatConversaRepository conversaRepository, IMediator mediator)
    {
        _conversaRepository = conversaRepository;
        _mediator = mediator;
    }

    public async Task Handle(MarcarComoLidoCommand request, CancellationToken cancellationToken)
    {
        var participante = await _conversaRepository.ObterParticipanteAsync(request.ConversaId, request.UsuarioId, cancellationToken);
        if (participante is null || !participante.Ativo)
            throw new ForbiddenException("Você não participa desta conversa.");

        participante.MarcarComoLido();
        await _conversaRepository.AtualizarParticipanteAsync(participante, cancellationToken);

        await _mediator.Publish(
            new ChatMensagemLidaNotification(request.ConversaId, request.UsuarioId, participante.UltimaLeituraEm ?? DateTime.UtcNow),
            cancellationToken);
    }
}
