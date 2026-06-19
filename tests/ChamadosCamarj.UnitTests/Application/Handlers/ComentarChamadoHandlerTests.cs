using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ComentarChamadoHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly ComentarChamadoCommandHandler _handler;

    public ComentarChamadoHandlerTests()
    {
        _handler = new ComentarChamadoCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DeveAdicionarComentarioPublicoEPersistir()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());

        _repositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new ComentarChamadoCommand(chamadoId, "Victor", "Estamos analisando.", false);
        await _handler.Handle(command, CancellationToken.None);

        chamado.Comentarios.Should().HaveCount(1);
        chamado.Comentarios.First().Tipo.Should().Be(TipoComentario.Publico);
        chamado.Comentarios.First().Autor.Should().Be("Victor");
        _repositoryMock.Verify(r => r.AtualizarAsync(chamado, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveAdicionarComentarioInterno_QuandoInternoTrue()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());

        _repositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new ComentarChamadoCommand(chamadoId, "Victor", "Nota interna.", true);
        await _handler.Handle(command, CancellationToken.None);

        chamado.Comentarios.First().Tipo.Should().Be(TipoComentario.Interno);
    }

    [Fact]
    public async Task Handle_QuandoChamadoNaoExiste_DeveLancarNotFoundException()
    {
        _repositoryMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado?)null);

        var command = new ComentarChamadoCommand(Guid.NewGuid(), "Victor", "Comentário.", false);
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
