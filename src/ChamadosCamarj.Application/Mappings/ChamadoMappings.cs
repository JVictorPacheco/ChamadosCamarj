using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Application.Mappings;

public static class ChamadoMappings
{
    public static ChamadoResponse ToResponse(this Chamado chamado) =>
        new(
            chamado.Id,
            chamado.Titulo,
            chamado.Descricao,
            chamado.Status,
            chamado.Prioridade,
            chamado.SolicitanteNome,
            chamado.SolicitanteEmail,
            chamado.ResponsavelId,
            chamado.ResponsavelNome,
            chamado.CategoriaId,
            chamado.Categoria?.Nome,
            chamado.DataLimite,
            chamado.DataConclusao,
            chamado.DataCriacao,
            chamado.DataAtualizacao,
            chamado.Comentarios.Count,
            chamado.Anexos.Count
        );
}
