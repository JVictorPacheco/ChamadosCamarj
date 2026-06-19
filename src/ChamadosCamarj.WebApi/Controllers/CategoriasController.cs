using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Features.Categorias.Queries;
using ChamadosCamarj.Application.Features.Categorias.DTOs;

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
    /// Lista categorias (apenas ativas por padrão)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoriaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoriaResponse>>> Listar(
        [FromQuery] bool? apenasAtivas = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ListarCategoriasQuery(apenasAtivas), cancellationToken);
        return Ok(result);
    }
}
