using ChamadosCamarj.Application.Features.Relatorios.Queries;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Relatorios;

public class ObterRelatorioMensalQueryHandlerTests
{
    private readonly Mock<IHistoricoRepository> _historicoRepositoryMock = new();
    private readonly ObterRelatorioMensalQueryHandler _handler;

    private static readonly Guid Fabio = Guid.NewGuid();
    private static readonly Guid Victor = Guid.NewGuid();

    public ObterRelatorioMensalQueryHandlerTests()
    {
        _handler = new ObterRelatorioMensalQueryHandler(_historicoRepositoryMock.Object);
        SetupEventosPadrao();
    }

    private static EventoRelatorioItem Evento(
        AcaoHistorico acao,
        DateTime dataHora,
        string categoria = "Atendimento",
        Guid? responsavelId = null,
        string? responsavelNome = null,
        DateTime? dataCriacao = null,
        DateTime? dataConclusao = null,
        DateTime? dataLimite = null)
    {
        return new EventoRelatorioItem(
            Guid.NewGuid(),
            acao,
            dataHora,
            categoria,
            responsavelId,
            responsavelNome,
            dataCriacao ?? dataHora,
            dataConclusao,
            dataLimite
        );
    }

    private void SetupEventos(DateTime inicio, DateTime fim, List<EventoRelatorioItem> eventos)
    {
        _historicoRepositoryMock
            .Setup(r => r.ObterEventosParaRelatorioAsync(
                It.IsAny<IEnumerable<AcaoHistorico>>(), inicio, fim, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventos);
    }

    private void SetupEventosPadrao()
    {
        _historicoRepositoryMock
            .Setup(r => r.ObterEventosParaRelatorioAsync(
                It.IsAny<IEnumerable<AcaoHistorico>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }

    [Fact]
    public async Task Handle_ComChamadosNoMes_DeveRetornarTotaisCorretos()
    {
        var inicio = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim = inicio.AddMonths(1);

        SetupEventos(inicio, fim, [
            Evento(AcaoHistorico.Criado, inicio.AddDays(1)),
            Evento(AcaoHistorico.Criado, inicio.AddDays(2)),
            Evento(AcaoHistorico.Resolvido, inicio.AddDays(3), dataCriacao: inicio.AddDays(1), dataConclusao: inicio.AddDays(3)),
            Evento(AcaoHistorico.Cancelado, inicio.AddDays(4)),
        ]);
        SetupEventos(inicio.AddMonths(-1), inicio, []);

        var result = await _handler.Handle(new ObterRelatorioMensalQuery(2026, 7), CancellationToken.None);

        result.TotalAbertos.Should().Be(2);
        result.TotalResolvidos.Should().Be(1);
        result.TotalCancelados.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ComMesVazio_DeveRetornarTudoZerado()
    {
        var inicio = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim = inicio.AddMonths(1);

        SetupEventos(inicio, fim, []);
        SetupEventos(inicio.AddMonths(-1), inicio, []);

        var result = await _handler.Handle(new ObterRelatorioMensalQuery(2026, 7), CancellationToken.None);

        result.TotalAbertos.Should().Be(0);
        result.TotalResolvidos.Should().Be(0);
        result.TotalCancelados.Should().Be(0);
        result.PorCategoria.Should().BeEmpty();
        result.Comparacao.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ComResponsavelId_DeveFiltrarEOmitirPorAtendente()
    {
        var inicio = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim = inicio.AddMonths(1);

        SetupEventos(inicio, fim, [
            Evento(AcaoHistorico.Criado, inicio.AddDays(1), responsavelId: Fabio, responsavelNome: "Fábio"),
            Evento(AcaoHistorico.Criado, inicio.AddDays(2), responsavelId: Victor, responsavelNome: "Victor"),
        ]);
        SetupEventos(inicio.AddMonths(-1), inicio, []);

        var result = await _handler.Handle(new ObterRelatorioMensalQuery(2026, 7, Fabio), CancellationToken.None);

        result.TotalAbertos.Should().Be(1);
        result.PorAtendente.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SemResponsavelId_DeveIncluirPorAtendente()
    {
        var inicio = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim = inicio.AddMonths(1);

        SetupEventos(inicio, fim, [
            Evento(AcaoHistorico.Criado, inicio.AddDays(1), responsavelId: Fabio, responsavelNome: "Fábio"),
            Evento(AcaoHistorico.Criado, inicio.AddDays(2), responsavelId: Victor, responsavelNome: "Victor"),
        ]);
        SetupEventos(inicio.AddMonths(-1), inicio, []);

        var result = await _handler.Handle(new ObterRelatorioMensalQuery(2026, 7), CancellationToken.None);

        result.PorAtendente.Should().NotBeNull();
        result.PorAtendente!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ComResolucoesDentroEForaDoPrazo_DeveCalcularSlaCorretamente()
    {
        var inicio = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim = inicio.AddMonths(1);

        SetupEventos(inicio, fim, [
            Evento(AcaoHistorico.Resolvido, inicio.AddDays(1),
                dataCriacao: inicio, dataConclusao: inicio.AddHours(4), dataLimite: inicio.AddHours(8)),
            Evento(AcaoHistorico.Resolvido, inicio.AddDays(2),
                dataCriacao: inicio, dataConclusao: inicio.AddHours(30), dataLimite: inicio.AddHours(8)),
        ]);
        SetupEventos(inicio.AddMonths(-1), inicio, []);

        var result = await _handler.Handle(new ObterRelatorioMensalQuery(2026, 7), CancellationToken.None);

        result.Sla.TotalComPrazo.Should().Be(2);
        result.Sla.DentroDoPrazo.Should().Be(1);
        result.Sla.Estourados.Should().Be(1);
        result.Sla.PercentualCumprido.Should().Be(50.0);
    }

    [Fact]
    public async Task Handle_SemMesAnteriorComDados_ComparacaoDeveSerNull()
    {
        var inicio = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim = inicio.AddMonths(1);

        SetupEventos(inicio, fim, [Evento(AcaoHistorico.Criado, inicio.AddDays(1))]);
        SetupEventos(inicio.AddMonths(-1), inicio, []);

        var result = await _handler.Handle(new ObterRelatorioMensalQuery(2026, 7), CancellationToken.None);

        result.Comparacao.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ComMesAnteriorComDados_DeveCalcularVariacaoPercentual()
    {
        var inicio = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim = inicio.AddMonths(1);

        SetupEventos(inicio, fim, [
            Evento(AcaoHistorico.Criado, inicio.AddDays(1)),
            Evento(AcaoHistorico.Criado, inicio.AddDays(2)),
        ]);
        SetupEventos(inicio.AddMonths(-1), inicio, [
            Evento(AcaoHistorico.Criado, inicio.AddMonths(-1).AddDays(1)),
        ]);

        var result = await _handler.Handle(new ObterRelatorioMensalQuery(2026, 7), CancellationToken.None);

        result.Comparacao.Should().NotBeNull();
        result.Comparacao!.VariacaoAbertosPercentual.Should().Be(100.0);
    }
}
