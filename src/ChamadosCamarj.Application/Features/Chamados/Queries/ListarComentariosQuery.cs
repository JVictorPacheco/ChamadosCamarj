using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public record ListarComentariosQuery(Guid ChamadoId) : IRequest<IEnumerable<ComentarioResponse>>;
