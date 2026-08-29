using MediatR;
using ChamadosCamarj.Application.Features.Chat.DTOs;

namespace ChamadosCamarj.Application.Features.Chat.Queries.ListarPresencas;

public record ListarPresencasQuery : IRequest<IEnumerable<ChatPresencaResponse>>;
