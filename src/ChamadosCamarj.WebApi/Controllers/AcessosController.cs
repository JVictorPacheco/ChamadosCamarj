using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Features.Acessos.Commands;
using ChamadosCamarj.Application.Features.Acessos.DTOs;
using ChamadosCamarj.Application.Features.Acessos.Queries;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.WebApi.Controllers;

/// <summary>
/// Controle de acesso por módulo (spec controle-de-acesso) — somente Admin. A checagem de perfil fica nos
/// handlers (PerfilRequisitanteGuard), como em UsuariosController.
/// </summary>
[ApiController]
[Route("api/acessos")]
public class AcessosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public AcessosController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>Lista as pessoas com os módulos de cada uma (Admins como acesso total).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AcessoUsuarioResumoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<AcessoUsuarioResumoResponse>>> Listar(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ListarAcessosQuery(_currentUser.Perfil), cancellationToken));

    /// <summary>Painel de uma pessoa: módulos, Chat e histórico de mudanças.</summary>
    [HttpGet("{usuarioId:guid}")]
    [ProducesResponseType(typeof(AcessoUsuarioDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AcessoUsuarioDetalheResponse>> Obter(Guid usuarioId, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ObterAcessoUsuarioQuery(usuarioId, _currentUser.Perfil), cancellationToken));

    /// <summary>Define os módulos e o nível do Chat da pessoa.</summary>
    [HttpPut("{usuarioId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Salvar(Guid usuarioId, [FromBody] SalvarAcessosRequest request, CancellationToken cancellationToken)
    {
        // Sem o Chat no pedido, recusa — nunca "revogar por omissão" (review R-05).
        if (request.Modulos is null || request.ChatPerfil is null)
            return BadRequest(new { message = "Informe os módulos e o nível de Chat." });

        await _mediator.Send(new SalvarAcessosCommand(
            usuarioId, request.Modulos, request.ChatPerfil.Value,
            _currentUser.Perfil, _currentUser.UsuarioId, _currentUser.Nome, request.PerfilEsperado), cancellationToken);
        return NoContent();
    }

    /// <summary>Ajusta só o Chat — vale também para Admins e para o próprio Admin.</summary>
    [HttpPut("{usuarioId:guid}/chat")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DefinirChat(Guid usuarioId, [FromBody] DefinirChatDeAcessoRequest request, CancellationToken cancellationToken)
    {
        if (request.ChatPerfil is null)
            return BadRequest(new { message = "Informe o nível de Chat." });

        await _mediator.Send(new DefinirChatDeAcessoCommand(usuarioId, request.ChatPerfil.Value, _currentUser.Perfil, _currentUser.UsuarioId, _currentUser.Nome), cancellationToken);
        return NoContent();
    }

    /// <summary>Volta a pessoa ao padrão de módulos do perfil (o Chat não muda).</summary>
    [HttpPost("{usuarioId:guid}/padrao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> VoltarAoPadrao(Guid usuarioId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new VoltarAoPadraoCommand(usuarioId, _currentUser.Perfil, _currentUser.UsuarioId, _currentUser.Nome), cancellationToken);
        return NoContent();
    }
}

public record SalvarAcessosRequest(IReadOnlyList<string>? Modulos, ChatPerfil? ChatPerfil, Perfil? PerfilEsperado = null);
public record DefinirChatDeAcessoRequest(ChatPerfil? ChatPerfil);
