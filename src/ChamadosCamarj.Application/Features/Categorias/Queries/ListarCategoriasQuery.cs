using MediatR;
using ChamadosCamarj.Application.Features.Categorias.DTOs;

namespace ChamadosCamarj.Application.Features.Categorias.Queries;

public record ListarCategoriasQuery(bool? ApenasAtivas = null) : IRequest<IEnumerable<CategoriaResponse>>;
