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
    List<PorCategoriaItem> PorCategoria,
    List<PorAtendenteItem>? PorAtendente,
    ComparacaoMesAnteriorResponse? Comparacao
);

public record SlaResponse(int TotalComPrazo, int DentroDoPrazo, int Estourados, double? PercentualCumprido);

public record PorCategoriaItem(string CategoriaNome, int Quantidade);

public record PorAtendenteItem(string ResponsavelNome, int Abertos, int Resolvidos, int Cancelados);

public record ComparacaoMesAnteriorResponse(
    double? VariacaoAbertosPercentual,
    double? VariacaoResolvidosPercentual,
    double? VariacaoCanceladosPercentual
);
