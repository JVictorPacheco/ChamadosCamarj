namespace ChamadosCamarj.Application.Features.Categorias.DTOs;

public record CategoriaResponse(
    Guid Id,
    string Nome,
    string Descricao,
    bool Ativa
);
