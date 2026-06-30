using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ComentarChamadoHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly ComentarChamadoCommandHandler _handler;

    public ComentarChamadoHandlerTests()
    {
        _handler = new ComentarChamadoCommandHandler(_repositoryMock.Object, _publisherMock.Object);
    }

    [Fact]
    public async Task Handle_DeveAdicionarComentarioPublicoEPersistir()
    {
        var chamadoId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.ExisteAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Comentario? comentarioAdicionado = null;
        _repositoryMock.Setup(r => r.AdicionarComentarioAsync(It.IsAny<Comentario>(), It.IsAny<CancellationToken>()))
            .Callback<Comentario, CancellationToken>((c, _) => comentarioAdicionado = c)
            .Returns(Task.CompletedTask);

        var command = new ComentarChamadoCommand(chamadoId, "Victor", "Estamos analisando.", false);
        await _handler.Handle(command, CancellationToken.None);

        comentarioAdicionado.Should().NotBeNull();
        comentarioAdicionado!.Tipo.Should().Be(TipoComentario.Publico);
        comentarioAdicionado.Autor.Should().Be("Victor");
        comentarioAdicionado.ChamadoId.Should().Be(chamadoId);
        _repositoryMock.Verify(r => r.AdicionarComentarioAsync(It.IsAny<Comentario>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveAdicionarComentarioInterno_QuandoInternoTrue()
    {
        var chamadoId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.ExisteAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Comentario? comentarioAdicionado = null;
        _repositoryMock.Setup(r => r.AdicionarComentarioAsync(It.IsAny<Comentario>(), It.IsAny<CancellationToken>()))
            .Callback<Comentario, CancellationToken>((c, _) => comentarioAdicionado = c)
            .Returns(Task.CompletedTask);

        var command = new ComentarChamadoCommand(chamadoId, "Victor", "Nota interna.", true);
        await _handler.Handle(command, CancellationToken.None);

        comentarioAdicionado!.Tipo.Should().Be(TipoComentario.Interno);
    }

    [Fact]
    public async Task Handle_QuandoChamadoNaoExiste_DeveLancarNotFoundException()
    {
        _repositoryMock.Setup(r => r.ExisteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new ComentarChamadoCommand(Guid.NewGuid(), "Victor", "Comentário.", false);
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
