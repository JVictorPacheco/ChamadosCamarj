using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Domain.Interfaces;
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
    private readonly ICategoriaRepository _categoriaRepository;

    public CategoriasController(IMediator mediator, ICurrentUserService currentUser, ICategoriaRepository categoriaRepository)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _categoriaRepository = categoriaRepository;
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

    /// <summary>
    /// Exclui uma categoria (somente Admin). Categorias com chamados vinculados não podem ser excluídas.
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

        var categoria = await _categoriaRepository.ObterPorIdAsync(id, cancellationToken);
        if (categoria is null)
            return NotFound(new { message = "Categoria não encontrada." });

        if (await _categoriaRepository.PossuiChamadosAsync(id, cancellationToken))
            return Conflict(new { message = "Não é possível excluir esta categoria porque existem chamados vinculados a ela. Desative-a em vez disso." });

        await _categoriaRepository.RemoverAsync(categoria, cancellationToken);

        return NoContent();
    }
}
