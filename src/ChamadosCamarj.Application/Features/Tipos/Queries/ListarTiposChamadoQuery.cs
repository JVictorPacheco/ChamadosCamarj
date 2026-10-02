using MediatR;
using ChamadosCamarj.Application.Features.Tipos.DTOs;

namespace ChamadosCamarj.Application.Features.Tipos.Queries;

public record ListarTiposChamadoQuery(bool? ApenasAtivos = null) : IRequest<IEnumerable<TipoChamadoResponse>>;
