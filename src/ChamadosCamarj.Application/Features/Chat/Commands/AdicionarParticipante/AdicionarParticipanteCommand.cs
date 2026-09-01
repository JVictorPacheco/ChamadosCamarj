using MediatR;

namespace ChamadosCamarj.Application.Features.Chat.Commands.AdicionarParticipante;

public record AdicionarParticipanteCommand(
    Guid ConversaId,
    Guid UsuarioId,
    Guid RequisitanteId = default,
    string RequisitanteNome = "",
    string? RequisitantePerfil = null
) : IRequest;
