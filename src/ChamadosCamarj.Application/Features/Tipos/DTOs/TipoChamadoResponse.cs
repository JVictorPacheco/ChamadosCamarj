namespace ChamadosCamarj.Application.Features.Tipos.DTOs;

public record TipoChamadoResponse(
    Guid Id,
    string Nome,
    string Descricao,
    bool Ativo
);
