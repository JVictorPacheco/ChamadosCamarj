using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

/// <summary>
/// spec correcoes-acesso-chamados AC-01: a edição usava ObterPorIdAsync (sem tracking) e todo save
/// caía no controle de concorrência (409).
/// </summary>
public class AtualizarChamadoHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly AtualizarChamadoCommandHandler _handler;

    public AtualizarChamadoHandlerTests()
    {
        _handler = new AtualizarChamadoCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DeveBuscarComTracking_EPersistirOsDadosNovos()
    {
        var id = Guid.NewGuid();
        var chamado = new Chamado("Antigo", "Descrição antiga", "Ana", "ana@camarj.com.br", Guid.NewGuid(), Guid.NewGuid());
        _repositoryMock.Setup(r => r.ObterPorIdComTrackingAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(chamado);

        await _handler.Handle(new AtualizarChamadoCommand(id, "Novo", "Descrição nova"), CancellationToken.None);

        _repositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.AtualizarAsync(
            It.Is<Chamado>(c => c.Titulo == "Novo" && c.Descricao == "Descrição nova"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoChamadoNaoExiste_DeveLancarNotFound()
    {
        _repositoryMock.Setup(r => r.ObterPorIdComTrackingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado?)null);

        var act = () => _handler.Handle(new AtualizarChamadoCommand(Guid.NewGuid(), "T", "D"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
