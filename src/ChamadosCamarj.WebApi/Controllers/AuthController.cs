using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Features.Auth.Commands;
using ChamadosCamarj.Application.Features.Auth.DTOs;
using ChamadosCamarj.Application.Features.Auth.Queries;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;

namespace ChamadosCamarj.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Autentica via Google Workspace: recebe o id_token do Google, valida, busca o
    /// perfil correspondente e emite um JWT próprio da aplicação.
    /// </summary>
    [HttpPost("google")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AutenticacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AutenticacaoResponse>> Google(
        [FromBody] AutenticarGoogleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Autentica via e-mail e senha cadastrados pelo Admin, emite um JWT próprio da aplicação.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AutenticacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AutenticacaoResponse>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Envia um email com link de redefinição de senha. Sempre retorna 200
    /// (mesmo que o email não exista) para evitar enumeração de contas.
    /// </summary>
    [HttpPost("esqueci-senha")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EsqueciSenha(
        [FromBody] EsqueciSenhaCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok(new { mensagem = "Se o e-mail estiver cadastrado, um link de redefinição será enviado." });
    }

    /// <summary>
    /// Redefine a senha de um usuário usando um token de redefinição enviado por email.
    /// </summary>
    [HttpPost("resetar-senha")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetarSenha(
        [FromBody] ResetarSenhaCommand command,
        CancellationToken cancellationToken)
    {
        var sucesso = await _mediator.Send(command, cancellationToken);
        if (!sucesso)
            return BadRequest(new { mensagem = "Link inválido ou expirado. Solicite uma nova redefinição." });

        return Ok(new { mensagem = "Senha redefinida com sucesso." });
    }

    /// <summary>
    /// Retorna o perfil atual do usuário autenticado, revalidado no banco (não confia só no que
    /// está no JWT/localStorage) — usado no boot do frontend pra refletir mudanças (ex: ChatPerfil
    /// revogado) que aconteceram enquanto a pessoa estava deslogada ou com a aba fechada (AC-48).
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UsuarioPerfilResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioPerfilResponse>> Me(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObterPerfilAtualQuery(_currentUser.UsuarioId), cancellationToken);
        return Ok(result);
    }
}
