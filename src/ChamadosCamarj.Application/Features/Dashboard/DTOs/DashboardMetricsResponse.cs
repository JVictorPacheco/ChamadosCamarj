namespace ChamadosCamarj.Application.Features.Dashboard.DTOs;

public record DashboardMetricsResponse(
    int TotalAbertos,
    int TotalEmAndamento,
    int TotalResolvidosHoje,
    double? TempoMedioResolucaoHoras,
    List<PorCategoriaItem> PorCategoria,
    List<PorPrioridadeItem> PorPrioridade
);

public record PorCategoriaItem(string CategoriaNome, int Quantidade);
public record PorPrioridadeItem(string Prioridade, int Quantidade);
