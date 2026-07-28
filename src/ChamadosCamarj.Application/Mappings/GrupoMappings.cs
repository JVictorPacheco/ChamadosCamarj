using ChamadosCamarj.Application.Features.Grupos.DTOs;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Application.Mappings;

public static class GrupoMappings
{
    public static GrupoResponse ToResponse(this Grupo grupo) =>
        new(
            grupo.Id,
            grupo.Nome,
            grupo.Descricao,
            grupo.Ativo
        );
}
