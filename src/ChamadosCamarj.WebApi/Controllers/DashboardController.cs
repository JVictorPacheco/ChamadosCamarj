using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Features.Dashboard.Queries;

namespace ChamadosCamarj.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retorna métricas agregadas do dashboard (KPIs, por categoria, por prioridade).
    /// </summary>
    [HttpGet("metricas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterMetricas(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObterMetricasQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retorna tendência diária de chamados abertos vs resolvidos.
    /// </summary>
    [HttpGet("tendencia")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTendencia([FromQuery] int dias = 7, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ObterTendenciaQuery(dias), cancellationToken);
        return Ok(result);
    }
}
