using MediatR;
using ChamadosCamarj.Application.Features.Grupos.DTOs;

namespace ChamadosCamarj.Application.Features.Grupos.Queries;

public record ListarGruposQuery() : IRequest<IEnumerable<GrupoResponse>>;
