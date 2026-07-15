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
        var result = await _mediator.Send(new ObterRelatorioMensalQuery(ano, mes, responsavelId), cancellationToken);
        return Ok(result);
    }
}
