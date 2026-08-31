using MediatR;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chat.Commands.DefinirChatPerfil;

public record DefinirChatPerfilCommand(
    Guid UsuarioId,
    ChatPerfil ChatPerfil,
    string PerfilRequisitante = "",
    Guid AdminId = default,
    string AdminNome = "Sistema"
) : IRequest;
