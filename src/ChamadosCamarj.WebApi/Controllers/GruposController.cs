using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Features.Grupos.Commands;
using ChamadosCamarj.Application.Features.Grupos.DTOs;
using ChamadosCamarj.Application.Features.Grupos.Queries;

namespace ChamadosCamarj.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GruposController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public GruposController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lista os grupos cadastrados
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GrupoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GrupoResponse>>> Listar(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListarGruposQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém um grupo pelo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GrupoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GrupoResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObterGrupoPorIdQuery(id), cancellationToken);

        if (result is null)
            return NotFound(new { message = "Grupo não encontrado." });

        return Ok(result);
    }

    /// <summary>
    /// Cria um novo grupo (somente Admin)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(GrupoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GrupoResponse>> Criar(
        [FromBody] CriarGrupoCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { PerfilRequisitante = _currentUser.Perfil }, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Atualiza um grupo existente (somente Admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarGrupoCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { Id = id, PerfilRequisitante = _currentUser.Perfil }, cancellationToken);

        if (result is null)
            return NotFound(new { message = "Grupo não encontrado." });

        return NoContent();
    }
}
