using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Features.Usuarios.Commands;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;
using ChamadosCamarj.Application.Features.Usuarios.Queries;

namespace ChamadosCamarj.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsuariosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista os usuários cadastrados (somente Admin)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UsuarioPerfilResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UsuarioPerfilResponse>>> Listar(
        [FromQuery] string? perfilRequisitante,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListarUsuariosPerfilQuery(perfilRequisitante), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cadastra um novo usuário (somente Admin)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioPerfilResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioPerfilResponse>> Criar(
        [FromQuery] string? perfilRequisitante,
        [FromBody] CriarUsuarioPerfilCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { PerfilRequisitante = perfilRequisitante }, cancellationToken);
        return CreatedAtAction(nameof(ObterPorEmail), new { email = result.Email }, result);
    }

    /// <summary>
    /// Atualiza nome, perfil e status de um usuário (somente Admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromQuery] string? perfilRequisitante,
        [FromBody] AtualizarUsuarioPerfilCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { Id = id, PerfilRequisitante = perfilRequisitante }, cancellationToken);

        if (result is null)
            return NotFound(new { message = "Usuário não encontrado." });

        return NoContent();
    }

    /// <summary>
    /// Busca um usuário pelo e-mail (usado pelo login, sem checagem de perfil)
    /// </summary>
    [HttpGet("por-email")]
    [ProducesResponseType(typeof(UsuarioPerfilResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioPerfilResponse>> ObterPorEmail(
        [FromQuery] string email,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObterUsuarioPerfilPorEmailQuery(email), cancellationToken);

        if (result is null)
            return NotFound(new { message = "Usuário não encontrado." });

        return Ok(result);
    }
}
