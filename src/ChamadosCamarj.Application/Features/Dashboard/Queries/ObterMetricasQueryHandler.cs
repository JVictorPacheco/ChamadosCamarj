using ChamadosCamarj.Application.Common.Autorizacao;
using MediatR;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Dashboard.DTOs;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Dashboard.Queries;

public class ObterMetricasQueryHandler : IRequestHandler<ObterMetricasQuery, DashboardMetricsResponse>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ModuloGuard _moduloGuard;

    public ObterMetricasQueryHandler(IChamadoRepository chamadoRepository, ICurrentUserService currentUser, ModuloGuard moduloGuard)
    {
        _moduloGuard = moduloGuard;
        _chamadoRepository = chamadoRepository;
        _currentUser = currentUser;
    }

    public async Task<DashboardMetricsResponse> Handle(ObterMetricasQuery request, CancellationToken cancellationToken)
    {
        // Dashboard é só de Atendente/Admin; os números contam só o que o usuário pode ver (AC-13).
        var acesso = _currentUser.ObterContextoAcesso();
        // Quem não tem o módulo Dashboard é recusado; quem tem vê só os chamados que já vê (a visibilidade
        // entra pelo ContextoAcesso). Spec controle-de-acesso AC-07/AC-12 — antes: "Solicitante → 403".
        await _moduloGuard.ExigirAsync(ModuloSistema.Dashboard, "Você não tem permissão para acessar o dashboard.", cancellationToken);

        var totalResolvidosHoje = await _chamadoRepository.ContarResolvidosHojeAsync(acesso, cancellationToken);
        var tempoMedio = await _chamadoRepository.ObterTempoMedioResolucaoHorasAsync(acesso, cancellationToken);
        var porArea = await _chamadoRepository.ContarPorAreaAsync(acesso, cancellationToken);
        var porTipo = await _chamadoRepository.ContarPorTipoAsync(acesso, cancellationToken);
        var porPrioridade = await _chamadoRepository.ContarPorPrioridadeAsync(acesso, cancellationToken);

        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var fimMes = inicioMes.AddMonths(1).AddTicks(-1);
        var sla = await _chamadoRepository.ContarSlaComplianceAsync(acesso, inicioMes, fimMes, cancellationToken);

        SlaComplianceItem? slaItem = sla.TotalResolvidos > 0
            ? new SlaComplianceItem(sla.TotalResolvidos, sla.DentroPrazo, Math.Round((double)sla.DentroPrazo / sla.TotalResolvidos * 100, 1))
            : null;

        return new DashboardMetricsResponse(
            totalResolvidosHoje,
            tempoMedio.HasValue ? Math.Round(tempoMedio.Value, 1) : null,
            porArea.Select(c => new PorNomeItem(c.Nome, c.Id, c.Quantidade)).ToList(),
            porTipo.Select(c => new PorNomeItem(c.Nome, c.Id, c.Quantidade)).ToList(),
            porPrioridade.Select(kvp => new PorPrioridadeItem(kvp.Key, kvp.Value)).ToList(),
            slaItem
        );
    }
}
