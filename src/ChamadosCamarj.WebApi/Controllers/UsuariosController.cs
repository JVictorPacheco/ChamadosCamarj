using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Common;
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
    private readonly ICurrentUserService _currentUser;

    public UsuariosController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lista os usuários cadastrados (somente Admin)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UsuarioPerfilResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UsuarioPerfilResponse>>> Listar(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListarUsuariosPerfilQuery(_currentUser.Perfil), cancellationToken);
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
        [FromBody] CriarUsuarioPerfilCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { PerfilRequisitante = _currentUser.Perfil }, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
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
        [FromBody] AtualizarUsuarioPerfilCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { Id = id, PerfilRequisitante = _currentUser.Perfil }, cancellationToken);

        if (result is null)
            return NotFound(new { message = "Usuário não encontrado." });

        return NoContent();
    }

    /// <summary>
    /// Redefine a senha de um usuário (somente Admin) — não depende de e-mail/token,
    /// o Admin define a nova senha diretamente
    /// </summary>
    [HttpPatch("{id:guid}/senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedefinirSenha(
        Guid id,
        [FromBody] RedefinirSenhaCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command with { Id = id, PerfilRequisitante = _currentUser.Perfil }, cancellationToken);
        return NoContent();
    }
}
