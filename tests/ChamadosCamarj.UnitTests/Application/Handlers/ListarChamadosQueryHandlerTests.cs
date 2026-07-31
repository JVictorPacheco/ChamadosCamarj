using ChamadosCamarj.Application.Features.Chamados.Queries;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ListarChamadosQueryHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly ListarChamadosQueryHandler _handler;

    public ListarChamadosQueryHandlerTests()
    {
        _handler = new ListarChamadosQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DevePassarSolicitanteEmailParaORepositorio()
    {
        _repositoryMock
            .Setup(r => r.ListarAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StatusChamado?>(), It.IsAny<PrioridadeChamado?>(),
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<IEnumerable<StatusChamado>?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<MotivoEncerramento?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<Chamado>(), 0));

        var query = new ListarChamadosQuery(SolicitanteEmail: "ana.colaboradora@camarj.com.br");
        await _handler.Handle(query, CancellationToken.None);

        _repositoryMock.Verify(r => r.ListarAsync(
            1, 10, null, null, null, null, null, "ana.colaboradora@camarj.com.br",
            null, null, null, null, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarApenasChamadosDoSolicitante()
    {
        var chamado = new Chamado("Título", "Descrição", "Ana", "ana.colaboradora@camarj.com.br", Guid.NewGuid());

        _repositoryMock
            .Setup(r => r.ListarAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StatusChamado?>(), It.IsAny<PrioridadeChamado?>(),
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), "ana.colaboradora@camarj.com.br",
                It.IsAny<IEnumerable<StatusChamado>?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<MotivoEncerramento?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { chamado }, 1));

        var query = new ListarChamadosQuery(SolicitanteEmail: "ana.colaboradora@camarj.com.br");
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Total.Should().Be(1);
        result.Items.Should().ContainSingle(c => c.SolicitanteEmail == "ana.colaboradora@camarj.com.br");
    }

    [Fact]
    public async Task Handle_ComFinalizadosTrue_DevePassarOsTresStatusFinalizadosParaORepositorio()
    {
        IEnumerable<StatusChamado>? statusCapturado = null;
        _repositoryMock
            .Setup(r => r.ListarAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StatusChamado?>(), It.IsAny<PrioridadeChamado?>(),
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<IEnumerable<StatusChamado>?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<MotivoEncerramento?>(), It.IsAny<CancellationToken>()))
            .Callback<int, int, StatusChamado?, PrioridadeChamado?, Guid?, Guid?, string?, string?, IEnumerable<StatusChamado>?, DateTime?, DateTime?, Guid?, Guid?, MotivoEncerramento?, CancellationToken>(
                (_, _, _, _, _, _, _, _, statusEntre, _, _, _, _, _, _) => statusCapturado = statusEntre)
            .ReturnsAsync((Enumerable.Empty<Chamado>(), 0));

        var query = new ListarChamadosQuery(Finalizados: true);
        await _handler.Handle(query, CancellationToken.None);

        statusCapturado.Should().BeEquivalentTo([StatusChamado.Resolvido, StatusChamado.Fechado, StatusChamado.Cancelado]);
    }

    [Fact]
    public async Task Handle_SemFinalizados_NaoDevePassarFiltroDeStatusEntre()
    {
        IEnumerable<StatusChamado>? statusCapturado = [StatusChamado.Aberto];
        _repositoryMock
            .Setup(r => r.ListarAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StatusChamado?>(), It.IsAny<PrioridadeChamado?>(),
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<IEnumerable<StatusChamado>?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<MotivoEncerramento?>(), It.IsAny<CancellationToken>()))
            .Callback<int, int, StatusChamado?, PrioridadeChamado?, Guid?, Guid?, string?, string?, IEnumerable<StatusChamado>?, DateTime?, DateTime?, Guid?, Guid?, MotivoEncerramento?, CancellationToken>(
                (_, _, _, _, _, _, _, _, statusEntre, _, _, _, _, _, _) => statusCapturado = statusEntre)
            .ReturnsAsync((Enumerable.Empty<Chamado>(), 0));

        var query = new ListarChamadosQuery();
        await _handler.Handle(query, CancellationToken.None);

        statusCapturado.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DevePassarDataInicioEDataFimComoUtcParaORepositorio()
    {
        var inicio = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var fim = new DateTime(2026, 7, 31, 0, 0, 0, DateTimeKind.Unspecified);

        DateTime? inicioCapturado = null;
        DateTime? fimCapturado = null;

        _repositoryMock
            .Setup(r => r.ListarAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StatusChamado?>(), It.IsAny<PrioridadeChamado?>(),
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<IEnumerable<StatusChamado>?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<MotivoEncerramento?>(), It.IsAny<CancellationToken>()))
            .Callback<int, int, StatusChamado?, PrioridadeChamado?, Guid?, Guid?, string?, string?, IEnumerable<StatusChamado>?, DateTime?, DateTime?, Guid?, Guid?, MotivoEncerramento?, CancellationToken>(
                (_, _, _, _, _, _, _, _, _, dataInicio, dataFim, _, _, _, _) =>
                {
                    inicioCapturado = dataInicio;
                    fimCapturado = dataFim;
                })
            .ReturnsAsync((Enumerable.Empty<Chamado>(), 0));

        var query = new ListarChamadosQuery(DataInicio: inicio, DataFim: fim);
        await _handler.Handle(query, CancellationToken.None);

        inicioCapturado!.Value.Kind.Should().Be(DateTimeKind.Utc);
        inicioCapturado!.Value.Should().Be(new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc));

        fimCapturado!.Value.Kind.Should().Be(DateTimeKind.Utc);
        fimCapturado!.Value.Date.Should().Be(new DateTime(2026, 7, 31, 0, 0, 0, DateTimeKind.Utc));
        fimCapturado!.Value.TimeOfDay.Should().BeGreaterThan(new TimeSpan(0, 23, 59, 59));
    }
}
