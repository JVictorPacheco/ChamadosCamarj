using MediatR;
using ChamadosCamarj.Application.Features.Chat.DTOs;

namespace ChamadosCamarj.Application.Features.Chat.Queries.ObterConversa;

public record ObterConversaQuery(Guid ConversaId, Guid UsuarioId) : IRequest<ChatConversaDetalheResponse>;
