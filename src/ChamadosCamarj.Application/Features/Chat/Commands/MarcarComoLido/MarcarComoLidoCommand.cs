using MediatR;

namespace ChamadosCamarj.Application.Features.Chat.Commands.MarcarComoLido;

public record MarcarComoLidoCommand(
    Guid ConversaId,
    Guid UsuarioId = default,
    string UsuarioNome = ""
) : IRequest;
