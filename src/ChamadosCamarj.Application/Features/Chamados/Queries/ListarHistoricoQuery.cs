using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public record ListarHistoricoQuery(Guid ChamadoId) : IRequest<IEnumerable<HistoricoResponse>>;
