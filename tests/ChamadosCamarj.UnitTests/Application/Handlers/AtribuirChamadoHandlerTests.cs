using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class AtribuirChamadoHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly AtribuirChamadoCommandHandler _handler;

    public AtribuirChamadoHandlerTests()
    {
        _handler = new AtribuirChamadoCommandHandler(_repositoryMock.Object, _publisherMock.Object);
    }

    [Fact]
    public async Task Handle_DeveAtribuirEPersistirChamado()
    {
        var chamadoId = Guid.NewGuid();
        var responsavelId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());

        _repositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new AtribuirChamadoCommand(chamadoId, responsavelId, "Victor");
        await _handler.Handle(command, CancellationToken.None);

        chamado.Status.Should().Be(StatusChamado.EmAndamento);
        chamado.ResponsavelId.Should().Be(responsavelId);
        chamado.ResponsavelNome.Should().Be("Victor");
        _repositoryMock.Verify(r => r.AtualizarAsync(chamado, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoChamadoNaoExiste_DeveLancarNotFoundException()
    {
        var chamadoId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado?)null);

        var command = new AtribuirChamadoCommand(chamadoId, Guid.NewGuid(), "Victor");

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
