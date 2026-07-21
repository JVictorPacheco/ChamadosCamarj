using ChamadosCamarj.Application.Features.Chamados.Queries;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ListarAnexosQueryHandlerTests
{
    private readonly Mock<IChamadoRepository> _chamadoRepositoryMock = new();
    private readonly ListarAnexosQueryHandler _handler;

    public ListarAnexosQueryHandlerTests()
    {
        _handler = new ListarAnexosQueryHandler(_chamadoRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarAnexosDoChamado()
    {
        var chamadoId = Guid.NewGuid();
        var anexo = new Anexo(chamadoId, "nota.pdf", $"{chamadoId}/abc.pdf", "application/pdf", 1024, Guid.NewGuid(), "Victor");

        _chamadoRepositoryMock.Setup(r => r.ObterAnexosPorChamadoAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([anexo]);

        var result = await _handler.Handle(new ListarAnexosQuery(chamadoId), CancellationToken.None);

        result.Should().ContainSingle();
        result.First().NomeArquivo.Should().Be("nota.pdf");
        result.First().EnviadoPorNome.Should().Be("Victor");
    }
}
