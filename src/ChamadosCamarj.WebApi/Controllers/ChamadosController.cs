using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Application.Features.Chamados.Queries;

namespace ChamadosCamarj.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChamadosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChamadosController> _logger;

    public ChamadosController(IMediator mediator, ILogger<ChamadosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os chamados com filtros opcionais
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ChamadoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ChamadoResponse>>> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? prioridade = null,
        [FromQuery] Guid? responsavelId = null,
        [FromQuery] Guid? categoriaId = null,
        [FromQuery] string? busca = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ListarChamadosQuery(pagina, tamanhoPagina, status, prioridade, responsavelId, categoriaId, busca);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém um chamado pelo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ChamadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChamadoResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObterChamadoPorIdQuery(id), cancellationToken);

        if (result == null)
            return NotFound(new { Message = "Chamado não encontrado." });

        return Ok(result);
    }

    /// <summary>
    /// Abre um novo chamado
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChamadoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChamadoResponse>> Abrir(
        [FromBody] AbrirChamadoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AbrirChamadoCommand(
            request.Titulo,
            request.Descricao,
            request.SolicitanteNome,
            request.SolicitanteEmail,
            request.CategoriaId,
            request.Prioridade);

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
    }

    /// <summary>
    /// Atualiza os dados de um chamado
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarChamadoRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new AtualizarChamadoCommand(id, request.Titulo, request.Descricao), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Atribui um chamado a um atendente
    /// </summary>
    [HttpPatch("{id:guid}/atribuir")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atribuir(
        Guid id,
        [FromBody] AtribuirChamadoCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command with { Id = id }, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Marca um chamado como resolvido
    /// </summary>
    [HttpPatch("{id:guid}/resolver")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Resolver(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ResolverChamadoCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Fecha um chamado resolvido
    /// </summary>
    [HttpPatch("{id:guid}/fechar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fechar(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new FecharChamadoCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Cancela um chamado aberto ou em andamento
    /// </summary>
    [HttpPatch("{id:guid}/cancelar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelarChamadoCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Adiciona um comentário a um chamado
    /// </summary>
    [HttpPost("{id:guid}/comentarios")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Comentar(
        Guid id,
        [FromBody] ComentarChamadoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ComentarChamadoCommand(id, request.Autor, request.Conteudo, request.Interno);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
