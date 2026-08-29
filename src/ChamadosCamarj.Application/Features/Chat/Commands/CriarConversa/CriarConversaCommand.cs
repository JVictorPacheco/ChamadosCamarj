using MediatR;
using ChamadosCamarj.Application.Features.Chat.DTOs;

namespace ChamadosCamarj.Application.Features.Chat.Commands.CriarConversa;

public record CriarConversaCommand(
    Guid DestinatarioId,
    Guid UsuarioId = default,
    string UsuarioNome = ""
) : IRequest<ChatConversaResponse>;
