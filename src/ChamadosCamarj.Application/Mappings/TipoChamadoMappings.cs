using ChamadosCamarj.Application.Features.Tipos.DTOs;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Application.Mappings;

public static class TipoChamadoMappings
{
    public static TipoChamadoResponse ToResponse(this TipoChamado tipo) =>
        new(
            tipo.Id,
            tipo.Nome,
            tipo.Descricao,
            tipo.Ativo
        );
}
