namespace ChamadosCamarj.Application.Features.Dashboard.DTOs;

public record DashboardMetricsResponse(
    int TotalResolvidosHoje,
    double? TempoMedioResolucaoHoras,
    List<PorCategoriaItem> PorCategoria,
    List<PorPrioridadeItem> PorPrioridade,
    SlaComplianceItem? SlaCompliance
);

public record SlaComplianceItem(int TotalResolvidos, int DentroPrazo, double Percentual);

public record PorCategoriaItem(string CategoriaNome, int Quantidade);
public record PorPrioridadeItem(string Prioridade, int Quantidade);
