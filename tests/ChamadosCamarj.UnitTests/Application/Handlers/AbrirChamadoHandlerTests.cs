using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class AbrirChamadoHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly AbrirChamadoCommandHandler _handler;

    public AbrirChamadoHandlerTests()
    {
        _repositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Chamado>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado c, CancellationToken _) => c);

        _handler = new AbrirChamadoCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DeveCriarChamadoERetornarResponse()
    {
        var categoriaId = Guid.NewGuid();
        var command = new AbrirChamadoCommand(
            "Problema de acesso",
            "Não consigo acessar o sistema.",
            "João",
            "joao@camarj.com.br",
            categoriaId,
            PrioridadeChamado.Alta);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Titulo.Should().Be("Problema de acesso");
        result.SolicitanteEmail.Should().Be("joao@camarj.com.br");
        result.Status.Should().Be(StatusChamado.Aberto);
        result.CategoriaId.Should().Be(categoriaId);
    }

    [Fact]
    public async Task Handle_DeveCallAdicionarAsyncUmaVez()
    {
        var command = new AbrirChamadoCommand(
            "Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Chamado>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComPrioridadeAlta_DeveCalcularDataLimiteEm24h()
    {
        var command = new AbrirChamadoCommand(
            "Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid(), PrioridadeChamado.Alta);

        var antes = DateTime.UtcNow;
        var result = await _handler.Handle(command, CancellationToken.None);

        result.DataLimite.Should().NotBeNull();
        result.DataLimite!.Value.Should().BeOnOrAfter(antes.AddHours(24));
        result.DataLimite!.Value.Should().BeOnOrBefore(DateTime.UtcNow.AddHours(24).AddSeconds(1));
    }
}
