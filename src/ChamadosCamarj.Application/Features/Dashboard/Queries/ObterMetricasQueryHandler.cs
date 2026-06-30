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
        var totalAbertos = await _chamadoRepository.ContarPorStatusAsync(Domain.Enums.StatusChamado.Aberto, cancellationToken);
        var totalEmAndamento = await _chamadoRepository.ContarPorStatusAsync(Domain.Enums.StatusChamado.EmAndamento, cancellationToken);
        var totalResolvidosHoje = await _chamadoRepository.ContarResolvidosHojeAsync(cancellationToken);
        var tempoMedio = await _chamadoRepository.ObterTempoMedioResolucaoHorasAsync(cancellationToken);
        var porCategoria = await _chamadoRepository.ContarPorCategoriaAsync(cancellationToken);
        var porPrioridade = await _chamadoRepository.ContarPorPrioridadeAsync(cancellationToken);

        return new DashboardMetricsResponse(
            totalAbertos,
            totalEmAndamento,
            totalResolvidosHoje,
            tempoMedio.HasValue ? Math.Round(tempoMedio.Value, 1) : null,
            porCategoria.Select(kvp => new PorCategoriaItem(kvp.Key, kvp.Value)).ToList(),
            porPrioridade.Select(kvp => new PorPrioridadeItem(kvp.Key, kvp.Value)).ToList()
        );
    }
}
