using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public record ListarAnexosQuery(Guid ChamadoId) : IRequest<IEnumerable<AnexoResponse>>;
