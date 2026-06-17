using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Features.Categorias.DTOs;
using ChamadosCamarj.Application.Features.Categorias.Queries;

namespace ChamadosCamarj.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriasController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista todas as categorias disponíveis
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoriaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoriaResponse>>> Listar(
        [FromQuery] bool? apenasAtivas = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ListarCategoriasQuery(apenasAtivas), cancellationToken);
        return Ok(result);
    }
}
