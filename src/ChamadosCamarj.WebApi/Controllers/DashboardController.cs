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
    /// Retorna a distribuição atual dos chamados por situação (Aguardando, Assumido, Resolvido, Cancelado).
    /// </summary>
    [HttpGet("distribuicao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterDistribuicao(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObterDistribuicaoQuery(), cancellationToken);
        return Ok(result);
    }
}
