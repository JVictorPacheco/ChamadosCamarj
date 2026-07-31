using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;
using ChamadosCamarj.Application.Common.Interfaces;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ReatribuirChamadoHandlerTests
{
    private readonly Mock<IChamadoRepository> _chamadoRepositoryMock = new();
    private readonly Mock<IHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ReatribuirChamadoCommandHandler _handler;

    public ReatribuirChamadoHandlerTests()
    {
        _handler = new ReatribuirChamadoCommandHandler(
            _chamadoRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _publisherMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_DeveReatribuirChamadoParaOutroAtendente()
    {
        var chamadoId = Guid.NewGuid();
        var atualResponsavelId = Guid.NewGuid();
        var novoResponsavelId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());
        
        // Atribuir primeiro
        chamado.Atribuir(atualResponsavelId, "Victor");
        
        _chamadoRepositoryMock.Setup(r => r.ObterPorIdComTrackingAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new ReatribuirChamadoCommand(chamadoId, novoResponsavelId, "Fábio");
        await _handler.Handle(command, CancellationToken.None);

        chamado.ResponsavelId.Should().Be(novoResponsavelId);
        chamado.ResponsavelNome.Should().Be("Fábio");
        chamado.Status.Should().Be(StatusChamado.EmAndamento);
        
        _chamadoRepositoryMock.Verify(r => r.AtualizarAsync(chamado, It.IsAny<CancellationToken>()), Times.Once);
        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<HistoricoEntrada>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveReatribuirChamadoAbertoPraEmAndamento()
    {
        var chamadoId = Guid.NewGuid();
        var novoResponsavelId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());
        // Chamado está aberto (sem responsável)

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdComTrackingAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new ReatribuirChamadoCommand(chamadoId, novoResponsavelId, "Victor");
        await _handler.Handle(command, CancellationToken.None);

        chamado.Status.Should().Be(StatusChamado.EmAndamento);
        chamado.ResponsavelNome.Should().Be("Victor");
    }

    [Fact]
    public async Task Handle_NaoDeveReatribuirChamadoFechado()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());
        chamado.Atribuir(Guid.NewGuid(), "Victor");
        chamado.Resolver();
        chamado.Fechar();

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdComTrackingAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new ReatribuirChamadoCommand(chamadoId, Guid.NewGuid(), "Fábio");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_QuandoChamadoNaoExiste_DeveLancarNotFoundException()
    {
        var chamadoId = Guid.NewGuid();
        _chamadoRepositoryMock.Setup(r => r.ObterPorIdComTrackingAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado?)null);

        var command = new ReatribuirChamadoCommand(chamadoId, Guid.NewGuid(), "Victor");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_DeveRegistrarHistoricoComDetalhesAnteriorENovo()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());
        chamado.Atribuir(Guid.NewGuid(), "Victor");

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdComTrackingAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new ReatribuirChamadoCommand(chamadoId, Guid.NewGuid(), "Fábio");
        await _handler.Handle(command, CancellationToken.None);

        _historicoRepositoryMock.Verify(
            r => r.AdicionarAsync(
                It.Is<HistoricoEntrada>(h => 
                    h.Acao == AcaoHistorico.Reatribuido &&
                    h.DetalheAnterior == "Victor" &&
                    h.DetalheNovo == "Fábio"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
