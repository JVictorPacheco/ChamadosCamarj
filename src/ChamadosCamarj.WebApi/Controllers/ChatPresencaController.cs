using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Features.Chat.Commands.AtualizarPresenca;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Application.Features.Chat.Queries.ListarPresencas;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.WebApi.Controllers;

/// <summary>
/// Presença é visível/atualizável por qualquer usuário autenticado, mesmo sem acesso ao chat.
/// </summary>
[ApiController]
[Route("api/chat")]
[Produces("application/json")]
public class ChatPresencaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public ChatPresencaController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lista o status de presença de todos os usuários ativos (acessível a qualquer perfil).
    /// </summary>
    [HttpGet("presencas")]
    [ProducesResponseType(typeof(IEnumerable<ChatPresencaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ChatPresencaResponse>>> ListarPresencas(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListarPresencasQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Heartbeat de presença: marca o usuário como Online (acessível a qualquer perfil).
    /// </summary>
    [HttpPost("presenca/heartbeat")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Heartbeat(CancellationToken cancellationToken)
    {
        var command = new AtualizarPresencaCommand(null, _currentUser.UsuarioId, _currentUser.Nome);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Define explicitamente o status de presença (ex.: Offline no logout).
    /// </summary>
    [HttpPost("presenca")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DefinirStatus(
        [FromBody] AtualizarPresencaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AtualizarPresencaCommand(request.Status, _currentUser.UsuarioId, _currentUser.Nome);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

public record AtualizarPresencaRequest(StatusPresenca? Status);
