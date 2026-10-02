using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Features.Tipos.Commands;
using ChamadosCamarj.Application.Features.Tipos.DTOs;
using ChamadosCamarj.Application.Features.Tipos.Queries;

namespace ChamadosCamarj.WebApi.Controllers;

[ApiController]
[Route("api/tipos")]
[Produces("application/json")]
public class TiposController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly ITipoChamadoRepository _tipoRepository;

    public TiposController(IMediator mediator, ICurrentUserService currentUser, ITipoChamadoRepository tipoRepository)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _tipoRepository = tipoRepository;
    }

    /// <summary>
    /// Lista tipos de chamado (apenas ativos por padrão)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TipoChamadoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TipoChamadoResponse>>> Listar(
        [FromQuery] bool? apenasAtivos = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ListarTiposChamadoQuery(apenasAtivos), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cria um novo tipo de chamado (somente Admin)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TipoChamadoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TipoChamadoResponse>> Criar(
        [FromBody] CriarTipoChamadoCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { PerfilRequisitante = _currentUser.Perfil }, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Atualiza um tipo existente (somente Admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarTipoChamadoCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command with { Id = id, PerfilRequisitante = _currentUser.Perfil }, cancellationToken);

        if (result is null)
            return NotFound(new { message = "Tipo de chamado não encontrado." });

        return NoContent();
    }

    /// <summary>
    /// Exclui um tipo (somente Admin). TipoChamados com chamados vinculados não podem ser excluídos.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(
        Guid id,
        CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(_currentUser.Perfil);

        var tipo = await _tipoRepository.ObterPorIdAsync(id, cancellationToken);
        if (tipo is null)
            return NotFound(new { message = "Tipo de chamado não encontrado." });

        if (await _tipoRepository.PossuiChamadosAsync(id, cancellationToken))
            return Conflict(new { message = "Não é possível excluir este tipo porque existem chamados vinculados a ele. Desative-o em vez disso." });

        await _tipoRepository.RemoverAsync(tipo, cancellationToken);

        return NoContent();
    }
}
