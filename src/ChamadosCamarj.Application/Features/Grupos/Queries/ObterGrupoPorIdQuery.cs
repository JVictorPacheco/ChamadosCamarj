using MediatR;
using ChamadosCamarj.Application.Features.Grupos.DTOs;

namespace ChamadosCamarj.Application.Features.Grupos.Queries;

public record ObterGrupoPorIdQuery(Guid Id) : IRequest<GrupoResponse?>;
