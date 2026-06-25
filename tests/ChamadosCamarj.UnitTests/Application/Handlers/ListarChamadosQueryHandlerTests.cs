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
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<Chamado>(), 0));

        var query = new ListarChamadosQuery(SolicitanteEmail: "ana.colaboradora@camarj.com.br");
        await _handler.Handle(query, CancellationToken.None);

        _repositoryMock.Verify(r => r.ListarAsync(
            1, 10, null, null, null, null, null, "ana.colaboradora@camarj.com.br", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarApenasChamadosDoSolicitante()
    {
        var chamado = new Chamado("Título", "Descrição", "Ana", "ana.colaboradora@camarj.com.br", Guid.NewGuid());

        _repositoryMock
            .Setup(r => r.ListarAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StatusChamado?>(), It.IsAny<PrioridadeChamado?>(),
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), "ana.colaboradora@camarj.com.br", It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { chamado }, 1));

        var query = new ListarChamadosQuery(SolicitanteEmail: "ana.colaboradora@camarj.com.br");
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Total.Should().Be(1);
        result.Items.Should().ContainSingle(c => c.SolicitanteEmail == "ana.colaboradora@camarj.com.br");
    }
}
