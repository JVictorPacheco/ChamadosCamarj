using MediatR;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ChamadosCamarj.Application.Features.Relatorios.Queries;
using ChamadosCamarj.Application.Features.Relatorios;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelatoriosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly ModuloGuard _moduloGuard;
    private readonly IChamadoRepository _chamadoRepository;

    public RelatoriosController(IMediator mediator, ICurrentUserService currentUser, ModuloGuard moduloGuard, IChamadoRepository chamadoRepository)
    {
        _moduloGuard = moduloGuard;
        _chamadoRepository = chamadoRepository;
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
        // Quem não tem o módulo é recusado (spec controle-de-acesso AC-12; antes: "Solicitante → 403").
        // Escopo dos números (autorizacao-chamados AC-21 + controle-de-acesso AC-07): Admin vê tudo;
        // Atendente só os próprios, qualquer que seja o responsavelId pedido; Solicitante com o módulo só os
        // chamados que ele vê.
        await _moduloGuard.ExigirAsync(ModuloSistema.RelatorioMensal, "Você não tem permissão para acessar o relatório.", cancellationToken);

        var (escopoResponsavel, idsVisiveis) = await RelatorioEscopo.ResolverAsync(
            _currentUser.ObterContextoAcesso(), responsavelId, _chamadoRepository, cancellationToken);

        var result = await _mediator.Send(new ObterRelatorioMensalQuery(ano, mes, escopoResponsavel, idsVisiveis), cancellationToken);
        return Ok(result);
    }
}
