namespace ChamadosCamarj.Application.Features.Relatorios.DTOs;

public record RelatorioMensalResponse(
    int Ano,
    int Mes,
    bool MesParcial,
    int TotalAbertos,
    int TotalResolvidos,
    int TotalCancelados,
    double? TempoMedioResolucaoHoras,
    SlaResponse Sla,
    List<PorNomeQuantidadeItem> PorArea,
    List<PorNomeQuantidadeItem> PorTipo,
    List<PorAtendenteItem>? PorAtendente,
    ComparacaoMesAnteriorResponse? Comparacao,
    List<SlaEvolucaoItem>? SlaEvolucao
);

public record SlaResponse(int TotalComPrazo, int DentroDoPrazo, int Estourados, double? PercentualCumprido);
public record SlaEvolucaoItem(int Ano, int Mes, double? Percentual);

public record PorNomeQuantidadeItem(string Nome, int Quantidade);

public record PorAtendenteItem(string ResponsavelNome, int Abertos, int Resolvidos, int Cancelados);

public record ComparacaoMesAnteriorResponse(
    double? VariacaoAbertosPercentual,
    double? VariacaoResolvidosPercentual,
    double? VariacaoCanceladosPercentual
);
