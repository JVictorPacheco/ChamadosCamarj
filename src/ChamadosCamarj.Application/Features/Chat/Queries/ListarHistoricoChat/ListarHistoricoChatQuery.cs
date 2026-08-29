using MediatR;
using ChamadosCamarj.Application.Features.Chat.DTOs;

namespace ChamadosCamarj.Application.Features.Chat.Queries.ListarHistoricoChat;

public record ListarHistoricoChatQuery(
    Guid? ConversaId = null,
    string PerfilRequisitante = ""
) : IRequest<IEnumerable<ChatHistoricoResponse>>;
