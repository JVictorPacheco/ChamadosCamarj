using MediatR;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Application.Features.Chat.Commands.AdicionarReacao;
using ChamadosCamarj.Application.Features.Chat.Commands.CriarConversa;
using ChamadosCamarj.Application.Features.Chat.Commands.CriarGrupo;
using ChamadosCamarj.Application.Features.Chat.Commands.DeletarMensagem;
using ChamadosCamarj.Application.Features.Chat.Commands.EditarMensagem;
using ChamadosCamarj.Application.Features.Chat.Commands.EnviarArquivo;
using ChamadosCamarj.Application.Features.Chat.Commands.EnviarMensagem;
using ChamadosCamarj.Application.Features.Chat.Commands.MarcarComoLido;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Application.Features.Chat.Queries.ListarConversas;
using ChamadosCamarj.Application.Features.Chat.Queries.ListarHistoricoChat;
using ChamadosCamarj.Application.Features.Chat.Queries.ListarMensagens;
using ChamadosCamarj.Application.Features.Chat.Queries.ObterArquivoMensagem;

namespace ChamadosCamarj.WebApi.Controllers;

[ApiController]
[Route("api/chat")]
[Produces("application/json")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public ChatController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lista as conversas do usuário autenticado com última mensagem e contagem de não lidas.
    /// </summary>
    [HttpGet("conversas")]
    [ProducesResponseType(typeof(IEnumerable<ChatConversaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ChatConversaResponse>>> ListarConversas(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListarConversasQuery(_currentUser.UsuarioId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cria (ou retorna, se já existir) uma conversa privada com outro usuário.
    /// </summary>
    [HttpPost("conversas")]
    [ProducesResponseType(typeof(ChatConversaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ChatConversaResponse>> CriarConversa(
        [FromBody] CriarConversaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CriarConversaCommand(request.DestinatarioId, _currentUser.UsuarioId, _currentUser.Nome);
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Cria um grupo de chat (apenas usuários com ChatPerfil CriadorDeGrupo).
    /// </summary>
    [HttpPost("grupos")]
    [ProducesResponseType(typeof(ChatConversaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ChatConversaResponse>> CriarGrupo(
        [FromBody] CriarGrupoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CriarGrupoCommand(request.Nome, request.ParticipanteIds, _currentUser.UsuarioId, _currentUser.Nome);
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Lista as mensagens de uma conversa (paginado, 20 por página, mais recentes primeiro).
    /// </summary>
    [HttpGet("conversas/{id:guid}/mensagens")]
    [ProducesResponseType(typeof(PagedResult<ChatMensagemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ChatMensagemResponse>>> ListarMensagens(
        Guid id,
        [FromQuery] int pagina = 1,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ListarMensagensQuery(id, pagina, _currentUser.UsuarioId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Envia uma mensagem de texto em uma conversa.
    /// </summary>
    [HttpPost("conversas/{id:guid}/mensagens")]
    [ProducesResponseType(typeof(ChatMensagemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ChatMensagemResponse>> EnviarMensagem(
        Guid id,
        [FromBody] EnviarMensagemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new EnviarMensagemCommand(
            id, request.Conteudo, request.RespostaParaMensagemId, _currentUser.UsuarioId, _currentUser.Nome);
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Envia um arquivo em uma conversa (PDF, imagens, Office, ZIP — máx 10MB).
    /// </summary>
    [HttpPost("conversas/{id:guid}/arquivos")]
    [ProducesResponseType(typeof(ChatMensagemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ChatMensagemResponse>> EnviarArquivo(
        Guid id,
        [FromForm] IFormFile arquivo,
        CancellationToken cancellationToken)
    {
        await using var conteudo = arquivo.OpenReadStream();
        var nomeSanitizado = Path.GetFileName(arquivo.FileName);
        var command = new EnviarArquivoCommand(
            id, nomeSanitizado, arquivo.ContentType, conteudo, arquivo.Length, _currentUser.UsuarioId, _currentUser.Nome);
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Gera uma URL assinada (válida por 1 hora) para download do arquivo de uma mensagem.
    /// </summary>
    [HttpGet("mensagens/{id:guid}/arquivo")]
    [ProducesResponseType(typeof(ChatArquivoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatArquivoResponse>> ObterArquivo(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObterArquivoMensagemQuery(id, _currentUser.UsuarioId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Edita uma mensagem própria (até 24h após o envio).
    /// </summary>
    [HttpPatch("mensagens/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditarMensagem(
        Guid id,
        [FromBody] EditarMensagemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new EditarMensagemCommand(id, request.Conteudo, _currentUser.UsuarioId, _currentUser.Nome);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deleta uma mensagem (autor ou Admin).
    /// </summary>
    [HttpDelete("mensagens/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletarMensagem(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeletarMensagemCommand(id, _currentUser.UsuarioId, _currentUser.Nome, _currentUser.Perfil);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Adiciona ou remove (toggle) uma reação de emoji em uma mensagem.
    /// </summary>
    [HttpPost("mensagens/{id:guid}/reacoes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdicionarReacao(
        Guid id,
        [FromBody] AdicionarReacaoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AdicionarReacaoCommand(id, request.Emoji, _currentUser.UsuarioId, _currentUser.Nome);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Marca a conversa como lida pelo usuário atual.
    /// </summary>
    [HttpPost("conversas/{id:guid}/leitura")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarcarComoLido(Guid id, CancellationToken cancellationToken)
    {
        var command = new MarcarComoLidoCommand(id, _currentUser.UsuarioId, _currentUser.Nome);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Lista o histórico de ações do chat (somente Admin). Filtra por conversa opcionalmente.
    /// </summary>
    [HttpGet("historico")]
    [ProducesResponseType(typeof(IEnumerable<ChatHistoricoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ChatHistoricoResponse>>> ListarHistorico(
        [FromQuery] Guid? conversaId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListarHistoricoChatQuery(conversaId, _currentUser.Perfil), cancellationToken);
        return Ok(result);
    }
}

public record CriarConversaRequest(Guid DestinatarioId);
public record CriarGrupoRequest(string Nome, IReadOnlyList<Guid> ParticipanteIds);
public record EnviarMensagemRequest(string Conteudo, Guid? RespostaParaMensagemId);
public record EditarMensagemRequest(string Conteudo);
public record AdicionarReacaoRequest(string Emoji);
