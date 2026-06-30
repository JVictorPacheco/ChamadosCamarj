using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ResolverFecharCancelarHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();

    private Chamado ChamadoAberto()
        => new("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());

    // ── Resolver ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Resolver_DeveMudarStatusEPersistir()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = ChamadoAberto();
        chamado.Atribuir(Guid.NewGuid(), "Victor");

        _repositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var handler = new ResolverChamadoCommandHandler(_repositoryMock.Object, _publisherMock.Object);
        await handler.Handle(new ResolverChamadoCommand(chamadoId), CancellationToken.None);

        chamado.Status.Should().Be(StatusChamado.Resolvido);
        chamado.DataConclusao.Should().NotBeNull();
        _repositoryMock.Verify(r => r.AtualizarAsync(chamado, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Resolver_QuandoNaoExiste_DeveLancarNotFoundException()
    {
        _repositoryMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado?)null);

        var handler = new ResolverChamadoCommandHandler(_repositoryMock.Object, _publisherMock.Object);
        var act = async () => await handler.Handle(new ResolverChamadoCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── Fechar ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Fechar_DeveMudarStatusParaFechadoEPersistir()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = ChamadoAberto();
        chamado.Atribuir(Guid.NewGuid(), "Victor");
        chamado.Resolver();

        _repositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var handler = new FecharChamadoCommandHandler(_repositoryMock.Object, _publisherMock.Object);
        await handler.Handle(new FecharChamadoCommand(chamadoId), CancellationToken.None);

        chamado.Status.Should().Be(StatusChamado.Fechado);
        _repositoryMock.Verify(r => r.AtualizarAsync(chamado, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Fechar_QuandoNaoExiste_DeveLancarNotFoundException()
    {
        _repositoryMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado?)null);

        var handler = new FecharChamadoCommandHandler(_repositoryMock.Object, _publisherMock.Object);
        var act = async () => await handler.Handle(new FecharChamadoCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── Cancelar ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Cancelar_DeveMudarStatusParaCanceladoEPersistir()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = ChamadoAberto();

        _repositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var handler = new CancelarChamadoCommandHandler(_repositoryMock.Object, _publisherMock.Object);
        await handler.Handle(new CancelarChamadoCommand(chamadoId), CancellationToken.None);

        chamado.Status.Should().Be(StatusChamado.Cancelado);
        _repositoryMock.Verify(r => r.AtualizarAsync(chamado, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancelar_QuandoNaoExiste_DeveLancarNotFoundException()
    {
        _repositoryMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado?)null);

        var handler = new CancelarChamadoCommandHandler(_repositoryMock.Object, _publisherMock.Object);
        var act = async () => await handler.Handle(new CancelarChamadoCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
