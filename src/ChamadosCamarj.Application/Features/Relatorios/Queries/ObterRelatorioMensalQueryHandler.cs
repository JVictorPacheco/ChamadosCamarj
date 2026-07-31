using MediatR;
using ChamadosCamarj.Application.Features.Relatorios.DTOs;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Relatorios.Queries;

public class ObterRelatorioMensalQueryHandler : IRequestHandler<ObterRelatorioMensalQuery, RelatorioMensalResponse>
{
    private static readonly AcaoHistorico[] AcoesRelevantes =
        [AcaoHistorico.Criado, AcaoHistorico.Resolvido, AcaoHistorico.Cancelado];

    private readonly IHistoricoRepository _historicoRepository;

    public ObterRelatorioMensalQueryHandler(IHistoricoRepository historicoRepository)
    {
        _historicoRepository = historicoRepository;
    }

    public async Task<RelatorioMensalResponse> Handle(ObterRelatorioMensalQuery request, CancellationToken cancellationToken)
    {
        var inicio = new DateTime(request.Ano, request.Mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var fimExclusivo = inicio.AddMonths(1);
        var agora = DateTime.UtcNow;
        var mesParcial = request.Ano == agora.Year && request.Mes == agora.Month;

        var eventos = await _historicoRepository.ObterEventosParaRelatorioAsync(
            AcoesRelevantes, inicio, fimExclusivo, cancellationToken);

        if (request.ResponsavelId is { } responsavelId)
        {
            eventos = eventos.Where(e => e.ResponsavelId == responsavelId).ToList();
        }

        var abertos = eventos.Where(e => e.Acao == AcaoHistorico.Criado).ToList();
        var resolvidos = eventos.Where(e => e.Acao == AcaoHistorico.Resolvido).ToList();
        var cancelados = eventos.Where(e => e.Acao == AcaoHistorico.Cancelado).ToList();

        var tempoMedio = resolvidos.Count > 0
            ? resolvidos.Average(e => (e.DataConclusao!.Value - e.DataCriacao).TotalHours)
            : (double?)null;

        var comSla = resolvidos.Where(e => e.DataLimite.HasValue).ToList();
        var dentroDoPrazo = comSla.Count(e => e.DataConclusao!.Value <= e.DataLimite!.Value);
        var estourados = comSla.Count - dentroDoPrazo;
        var sla = new SlaResponse(
            comSla.Count,
            dentroDoPrazo,
            estourados,
            comSla.Count > 0 ? Math.Round(dentroDoPrazo * 100.0 / comSla.Count, 1) : null
        );

        var porCategoria = abertos
            .GroupBy(e => e.CategoriaNome)
            .Select(g => new PorCategoriaItem(g.Key, g.Count()))
            .OrderByDescending(item => item.Quantidade)
            .ToList();

        List<PorAtendenteItem>? porAtendente = null;
        if (request.ResponsavelId is null)
        {
            porAtendente = eventos
                .GroupBy(e => e.ResponsavelNome ?? "Não atribuído")
                .Select(g => new PorAtendenteItem(
                    g.Key,
                    g.Count(e => e.Acao == AcaoHistorico.Criado),
                    g.Count(e => e.Acao == AcaoHistorico.Resolvido),
                    g.Count(e => e.Acao == AcaoHistorico.Cancelado)
                ))
                .OrderByDescending(item => item.Abertos + item.Resolvidos + item.Cancelados)
                .ToList();
        }

        var comparacao = await CalcularComparacaoAsync(request, inicio, abertos.Count, resolvidos.Count, cancelados.Count, cancellationToken);

        // SLA evolution: last 6 months
        var slaEvolucao = new List<SlaEvolucaoItem>();
        for (int i = 5; i >= 0; i--)
        {
            var mesEvol = inicio.AddMonths(-i);
            var fimEvol = mesEvol.AddMonths(1);
            var eventosEvol = await _historicoRepository.ObterEventosParaRelatorioAsync(
                [AcaoHistorico.Resolvido], mesEvol, fimEvol, cancellationToken);
            if (request.ResponsavelId is { } respId)
                eventosEvol = eventosEvol.Where(e => e.ResponsavelId == respId).ToList();

            var comSlaEvol = eventosEvol.Where(e => e.DataLimite.HasValue).ToList();
            var dentroEvol = comSlaEvol.Count(e => e.DataConclusao!.Value <= e.DataLimite!.Value);
            var pctEvol = comSlaEvol.Count > 0
                ? Math.Round(dentroEvol * 100.0 / comSlaEvol.Count, 1)
                : (double?)null;

            slaEvolucao.Add(new SlaEvolucaoItem(mesEvol.Year, mesEvol.Month, pctEvol));
        }

        return new RelatorioMensalResponse(
            request.Ano,
            request.Mes,
            mesParcial,
            abertos.Count,
            resolvidos.Count,
            cancelados.Count,
            tempoMedio.HasValue ? Math.Round(tempoMedio.Value, 1) : null,
            sla,
            porCategoria,
            porAtendente,
            comparacao,
            slaEvolucao
        );
    }

    private async Task<ComparacaoMesAnteriorResponse?> CalcularComparacaoAsync(
        ObterRelatorioMensalQuery request,
        DateTime inicioMesAtual,
        int abertosAtual,
        int resolvidosAtual,
        int canceladosAtual,
        CancellationToken cancellationToken)
    {
        var inicioMesAnterior = inicioMesAtual.AddMonths(-1);
        var fimMesAnteriorExclusivo = inicioMesAtual;

        var eventosMesAnterior = await _historicoRepository.ObterEventosParaRelatorioAsync(
            AcoesRelevantes, inicioMesAnterior, fimMesAnteriorExclusivo, cancellationToken);

        if (request.ResponsavelId is { } responsavelId)
        {
            eventosMesAnterior = eventosMesAnterior.Where(e => e.ResponsavelId == responsavelId).ToList();
        }

        if (eventosMesAnterior.Count == 0)
        {
            return null;
        }

        var abertosAnterior = eventosMesAnterior.Count(e => e.Acao == AcaoHistorico.Criado);
        var resolvidosAnterior = eventosMesAnterior.Count(e => e.Acao == AcaoHistorico.Resolvido);
        var canceladosAnterior = eventosMesAnterior.Count(e => e.Acao == AcaoHistorico.Cancelado);

        return new ComparacaoMesAnteriorResponse(
            CalcularVariacaoPercentual(abertosAtual, abertosAnterior),
            CalcularVariacaoPercentual(resolvidosAtual, resolvidosAnterior),
            CalcularVariacaoPercentual(canceladosAtual, canceladosAnterior)
        );
    }

    private static double? CalcularVariacaoPercentual(int atual, int anterior)
    {
        if (anterior == 0)
        {
            return null;
        }

        return Math.Round((atual - anterior) * 100.0 / anterior, 1);
    }
}
