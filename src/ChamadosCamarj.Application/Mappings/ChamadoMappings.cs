using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Mappings;

public static class ChamadoMappings
{
    /// <param name="incluirInternos">false para Solicitante: a contagem de comentários não pode
    /// revelar comentários internos (spec correcoes-acesso-chamados AC-02). Sem valor-padrão de
    /// propósito: quem mapeia um chamado precisa decidir conscientemente.</param>
    public static ChamadoResponse ToResponse(this Chamado chamado, bool incluirInternos) =>
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
            incluirInternos
                ? chamado.Comentarios.Count
                : chamado.Comentarios.Count(c => c.Tipo == TipoComentario.Publico),
            chamado.Anexos.Count,
            SlaCalculo.CalcularStatus(chamado.DataLimite),
            SlaCalculo.FormatarLabel(chamado.DataLimite),
            SlaCalculo.CalcularHorasRestantes(chamado.DataLimite),
            chamado.MotivoEncerramento,
            chamado.MotivoOutro
        );
}
