using ChamadosCamarj.Application.Features.Chamados.Queries;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ObterChamadoPorIdHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly ObterChamadoPorIdQueryHandler _handler;

    public ObterChamadoPorIdHandlerTests()
    {
        _handler = new ObterChamadoPorIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_QuandoChamadoExiste_DeveRetornarResponse()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());

        _repositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var result = await _handler.Handle(new ObterChamadoPorIdQuery(chamadoId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Titulo.Should().Be("Título");
    }

    [Fact]
    public async Task Handle_QuandoChamadoNaoExiste_DeveRetornarNull()
    {
        _repositoryMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado?)null);

        var result = await _handler.Handle(new ObterChamadoPorIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
