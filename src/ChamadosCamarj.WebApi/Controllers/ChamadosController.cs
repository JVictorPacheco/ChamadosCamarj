using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Common;
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
    private readonly ICurrentUserService _currentUser;

    public ChamadosController(IMediator mediator, ILogger<ChamadosController> logger, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _logger = logger;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lista chamados com filtros e paginação
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ChamadoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ChamadoResponse>>> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? prioridade = null,
        [FromQuery] Guid? responsavelId = null,
        [FromQuery] Guid? categoriaId = null,
        [FromQuery] string? busca = null,
        [FromQuery] string? solicitanteEmail = null,
        [FromQuery] bool? finalizados = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ListarChamadosQuery(pagina, tamanhoPagina, status, prioridade, responsavelId, categoriaId, busca, solicitanteEmail, finalizados, dataInicio, dataFim);
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
    /// Assume um chamado (atribui a si mesmo, o usuário autenticado)
    /// </summary>
    [HttpPatch("{id:guid}/atribuir")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atribuir(Guid id, CancellationToken cancellationToken)
    {
        var command = new AtribuirChamadoCommand(id, _currentUser.UsuarioId, _currentUser.Nome);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Reatribui um chamado para outro atendente (Admin)
    /// </summary>
    [HttpPatch("{id:guid}/reatribuir")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reatribuir(
        Guid id,
        [FromBody] ReatribuirRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReatribuirChamadoCommand(id, request.NovoResponsavelId, request.NovoResponsavelNome, _currentUser.UsuarioId, _currentUser.Nome);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Altera a prioridade de um chamado (Admin)
    /// </summary>
    [HttpPatch("{id:guid}/prioridade")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarPrioridade(
        Guid id,
        [FromBody] AlterarPrioridadeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AlterarPrioridadeChamadoCommand(id, request.NovaPrioridade, _currentUser.UsuarioId, _currentUser.Nome);
        await _mediator.Send(command, cancellationToken);
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
        await _mediator.Send(new ResolverChamadoCommand(id, _currentUser.UsuarioId, _currentUser.Nome), cancellationToken);
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
        await _mediator.Send(new FecharChamadoCommand(id, _currentUser.UsuarioId, _currentUser.Nome), cancellationToken);
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
        await _mediator.Send(new CancelarChamadoCommand(id, _currentUser.UsuarioId, _currentUser.Nome), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Força o encerramento de um chamado a partir de qualquer status não-final, com justificativa obrigatória (Admin)
    /// </summary>
    [HttpPatch("{id:guid}/forcar-encerramento")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForcarEncerramento(
        Guid id,
        [FromBody] ForcarEncerramentoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ForcarEncerramentoChamadoCommand(id, request.Motivo, _currentUser.UsuarioId, _currentUser.Nome, _currentUser.Perfil);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Altera o status de um chamado (usado pelo Kanban drag & drop)
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarStatus(Guid id, [FromBody] AlterarStatusRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AlterarStatusChamadoCommand(id, request.NovoStatus, _currentUser.UsuarioId, _currentUser.Nome), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Lista os comentários de um chamado
    /// </summary>
    [HttpGet("{id:guid}/comentarios")]
    [ProducesResponseType(typeof(IEnumerable<ComentarioResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ComentarioResponse>>> ListarComentarios(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListarComentariosQuery(id, _currentUser.Perfil), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lista o histórico de ações de um chamado
    /// </summary>
    [HttpGet("{id:guid}/historico")]
    [ProducesResponseType(typeof(IEnumerable<HistoricoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<HistoricoResponse>>> ListarHistorico(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListarHistoricoQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Adiciona um comentário a um chamado
    /// </summary>
    [HttpPost("{id:guid}/comentarios")]
    [ProducesResponseType(typeof(ComentarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ComentarioResponse>> Comentar(
        Guid id,
        [FromBody] ComentarChamadoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ComentarChamadoCommand(id, request.Autor, request.Conteudo, request.Interno);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(ListarComentarios), new { id }, result);
    }

    /// <summary>
    /// Anexa um arquivo a um chamado (ou a um comentário específico)
    /// </summary>
    [HttpPost("{id:guid}/anexos")]
    [ProducesResponseType(typeof(AnexoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<AnexoResponse>> AdicionarAnexo(
        Guid id,
        [FromForm] IFormFile arquivo,
        [FromForm] Guid? comentarioId,
        CancellationToken cancellationToken)
    {
        await using var conteudo = arquivo.OpenReadStream();
        // Nunca confiar no FileName do IFormFile sem sanitizar — um cliente malicioso pode
        // mandar um valor com segmentos de caminho embutidos (ex: "../../etc/x.pdf").
        var nomeArquivoSanitizado = Path.GetFileName(arquivo.FileName);
        var command = new AdicionarAnexoCommand(
            id,
            comentarioId,
            nomeArquivoSanitizado,
            arquivo.ContentType,
            conteudo,
            arquivo.Length,
            _currentUser.UsuarioId,
            _currentUser.Nome);

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(ListarAnexos), new { id }, result);
    }

    /// <summary>
    /// Lista os anexos de um chamado
    /// </summary>
    [HttpGet("{id:guid}/anexos")]
    [ProducesResponseType(typeof(IEnumerable<AnexoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AnexoResponse>>> ListarAnexos(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListarAnexosQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gera uma URL assinada (expira em 1h) pra baixar um anexo
    /// </summary>
    [HttpGet("{id:guid}/anexos/{anexoId:guid}/download-url")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterUrlDownloadAnexo(Guid id, Guid anexoId, CancellationToken cancellationToken)
    {
        var url = await _mediator.Send(new ObterUrlDownloadAnexoQuery(anexoId), cancellationToken);
        return Ok(new { url });
    }

    /// <summary>
    /// Remove um anexo (do Storage e do banco) — exclusão definitiva, sem desfazer
    /// </summary>
    [HttpDelete("{id:guid}/anexos/{anexoId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverAnexo(Guid id, Guid anexoId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoverAnexoCommand(id, anexoId, _currentUser.UsuarioId, _currentUser.Perfil), cancellationToken);
        return NoContent();
    }
}
