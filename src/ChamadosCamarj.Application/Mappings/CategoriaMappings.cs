using ChamadosCamarj.Application.Features.Categorias.DTOs;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Application.Mappings;

public static class CategoriaMappings
{
    public static CategoriaResponse ToResponse(this Categoria categoria) =>
        new(
            categoria.Id,
            categoria.Nome,
            categoria.Descricao,
            categoria.Ativa
        );
}
