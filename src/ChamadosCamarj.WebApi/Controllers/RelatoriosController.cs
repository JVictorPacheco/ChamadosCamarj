using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Features.Relatorios.Queries;

namespace ChamadosCamarj.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelatoriosController : ControllerBase
{
    private readonly IMediator _mediator;

    public RelatoriosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retorna o relatório mensal de chamados (totais, quebras, SLA, comparação com o mês anterior).
    /// </summary>
    [HttpGet("mensal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterRelatorioMensal(
        [FromQuery] int ano,
        [FromQuery] int mes,
        [FromQuery] Guid? responsavelId,
        CancellationToken cancellationToken)
    {
        // Quem pode ver e de quem são os números (Admin tudo; Atendente os próprios; Solicitante com o módulo
        // os que vê) — decidido em ObterRelatorioMensalAutorizadoQueryHandler (controle-de-acesso, review-2 R-06).
        var result = await _mediator.Send(new ObterRelatorioMensalAutorizadoQuery(ano, mes, responsavelId), cancellationToken);
        return Ok(result);
    }
}
