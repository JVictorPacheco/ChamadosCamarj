namespace ChamadosCamarj.Application.Features.Grupos.DTOs;

public record GrupoResponse(
    Guid Id,
    string Nome,
    string Descricao,
    bool Ativo
);
