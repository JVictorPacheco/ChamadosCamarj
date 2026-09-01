using MediatR;
using ChamadosCamarj.Application.Features.Chat.DTOs;

namespace ChamadosCamarj.Application.Features.Chat.Queries.ListarConversas;

public record ListarConversasQuery(
    Guid UsuarioId
) : IRequest<IEnumerable<ChatConversaResponse>>;
