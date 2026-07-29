using MediatR;
using ChamadosCamarj.Application.Features.Categorias.DTOs;

namespace ChamadosCamarj.Application.Features.Categorias.Commands;

public record CriarCategoriaCommand(
    string Nome,
    string Descricao,
    string? PerfilRequisitante = null
) : IRequest<CategoriaResponse>;
