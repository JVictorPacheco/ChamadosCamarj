using MediatR;

namespace ChamadosCamarj.Application.Features.Chat.Commands.RemoverParticipante;

public record RemoverParticipanteCommand(
    Guid ConversaId,
    Guid UsuarioId,
    Guid RequisitanteId = default,
    string RequisitanteNome = "",
    string? RequisitantePerfil = null
) : IRequest;
