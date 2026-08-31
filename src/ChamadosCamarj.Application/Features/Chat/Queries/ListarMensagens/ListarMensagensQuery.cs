using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Application.Features.Chat.DTOs;

namespace ChamadosCamarj.Application.Features.Chat.Queries.ListarMensagens;

public record ListarMensagensQuery(
    Guid ConversaId,
    int Pagina = 1,
    Guid UsuarioId = default
) : IRequest<PagedResult<ChatMensagemResponse>>;
