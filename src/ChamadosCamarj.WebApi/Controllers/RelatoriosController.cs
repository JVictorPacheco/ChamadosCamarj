using MediatR;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Features.Relatorios.Queries;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelatoriosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public RelatoriosController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Retorna o relatório mensal de chamados (totais, quebras, SLA, comparação com o mês anterior).
    /// </summary>
    [HttpGet("mensal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterRelatorioMensal(
        [FromQuery] int ano,
        [FromQuery] int mes,
        [FromQuery] Guid? responsavelId,
        CancellationToken cancellationToken)
    {
        // Spec autorizacao-chamados AC-21: Solicitante não acessa; Atendente só vê os próprios
        // números, qualquer que seja o responsavelId pedido; Admin vê o relatório completo.
        var acesso = _currentUser.ObterContextoAcesso();
        if (acesso.Perfil == Perfil.Solicitante)
            throw new ForbiddenException("Você não tem permissão para acessar o relatório.");
        if (acesso.Perfil == Perfil.Atendente)
            responsavelId = acesso.UsuarioId;

        var result = await _mediator.Send(new ObterRelatorioMensalQuery(ano, mes, responsavelId), cancellationToken);
        return Ok(result);
    }
}
