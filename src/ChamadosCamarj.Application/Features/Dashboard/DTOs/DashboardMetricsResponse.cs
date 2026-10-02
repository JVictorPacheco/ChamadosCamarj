namespace ChamadosCamarj.Application.Features.Dashboard.DTOs;

public record DashboardMetricsResponse(
    int TotalResolvidosHoje,
    double? TempoMedioResolucaoHoras,
    List<PorNomeItem> PorArea,
    List<PorNomeItem> PorTipo,
    List<PorPrioridadeItem> PorPrioridade,
    SlaComplianceItem? SlaCompliance
);

public record SlaComplianceItem(int TotalResolvidos, int DentroPrazo, double Percentual);

public record PorNomeItem(string Nome, Guid? Id, int Quantidade);
public record PorPrioridadeItem(string Prioridade, int Quantidade);
