using MediatR;
using ChamadosCamarj.Application.Features.Dashboard.DTOs;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Dashboard.Queries;

public class ObterMetricasQueryHandler : IRequestHandler<ObterMetricasQuery, DashboardMetricsResponse>
{
    private readonly IChamadoRepository _chamadoRepository;

    public ObterMetricasQueryHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task<DashboardMetricsResponse> Handle(ObterMetricasQuery request, CancellationToken cancellationToken)
    {
        var totalResolvidosHoje = await _chamadoRepository.ContarResolvidosHojeAsync(cancellationToken);
        var tempoMedio = await _chamadoRepository.ObterTempoMedioResolucaoHorasAsync(cancellationToken);
        var porCategoria = await _chamadoRepository.ContarPorCategoriaAsync(cancellationToken);
        var porPrioridade = await _chamadoRepository.ContarPorPrioridadeAsync(cancellationToken);

        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var fimMes = inicioMes.AddMonths(1).AddTicks(-1);
        var sla = await _chamadoRepository.ContarSlaComplianceAsync(inicioMes, fimMes, cancellationToken);

        SlaComplianceItem? slaItem = sla.TotalResolvidos > 0
            ? new SlaComplianceItem(sla.TotalResolvidos, sla.DentroPrazo, Math.Round((double)sla.DentroPrazo / sla.TotalResolvidos * 100, 1))
            : null;

        return new DashboardMetricsResponse(
            totalResolvidosHoje,
            tempoMedio.HasValue ? Math.Round(tempoMedio.Value, 1) : null,
            porCategoria.Select(kvp => new PorCategoriaItem(kvp.Key, kvp.Value)).ToList(),
            porPrioridade.Select(kvp => new PorPrioridadeItem(kvp.Key, kvp.Value)).ToList(),
            slaItem
        );
    }
}
