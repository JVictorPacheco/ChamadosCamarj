using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Features.Categorias.Commands;
using ChamadosCamarj.Application.Features.Categorias.DTOs;
using ChamadosCamarj.Application.Features.Categorias.Queries;

namespace ChamadosCamarj.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public CategoriasController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
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

    /// <summary>
    /// Cria uma nova categoria (somente Admin)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoriaResponse>> Criar(
        [FromBody] CriarCategoriaCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { PerfilRequisitante = _currentUser.Perfil }, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Atualiza uma categoria existente (somente Admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarCategoriaCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { Id = id, PerfilRequisitante = _currentUser.Perfil }, cancellationToken);

        if (result is null)
            return NotFound(new { message = "Categoria não encontrada." });

        return NoContent();
    }
}
