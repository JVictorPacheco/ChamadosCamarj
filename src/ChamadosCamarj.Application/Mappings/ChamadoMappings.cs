using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Mappings;

public static class ChamadoMappings
{
    public static ChamadoResponse ToResponse(this Chamado chamado) =>
        new(
            chamado.Id,
            chamado.Numero,
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
            chamado.Anexos.Count,
            SlaCalculo.CalcularStatus(chamado.DataLimite),
            SlaCalculo.FormatarLabel(chamado.DataLimite),
            SlaCalculo.CalcularHorasRestantes(chamado.DataLimite),
            chamado.MotivoEncerramento,
            chamado.MotivoOutro
        );
}
