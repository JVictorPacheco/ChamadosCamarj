using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class AlterarPrioridadeHandlerTests
{
    private readonly Mock<IChamadoRepository> _chamadoRepositoryMock = new();
    private readonly Mock<IHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly AlterarPrioridadeChamadoCommandHandler _handler;

    public AlterarPrioridadeHandlerTests()
    {
        _handler = new AlterarPrioridadeChamadoCommandHandler(
            _chamadoRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _publisherMock.Object);
    }

    [Fact]
    public async Task Handle_DeveAlterarPrioridadeDeMediaPraUrgente()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid(), PrioridadeChamado.Media);

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new AlterarPrioridadeChamadoCommand(chamadoId, "Urgente");
        await _handler.Handle(command, CancellationToken.None);

        chamado.Prioridade.Should().Be(PrioridadeChamado.Urgente);
        _chamadoRepositoryMock.Verify(r => r.AtualizarAsync(chamado, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveAlterarDataLimiteAoMudarPrioridade()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid(), PrioridadeChamado.Baixa);
        var dataLimiteAnterior = chamado.DataLimite;

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new AlterarPrioridadeChamadoCommand(chamadoId, "Urgente");
        await _handler.Handle(command, CancellationToken.None);

        chamado.DataLimite.Should().NotBe(dataLimiteAnterior);
        chamado.DataLimite.Should().BeLessThan(dataLimiteAnterior);
    }

    [Theory]
    [InlineData("Urgente")]
    [InlineData("Alta")]
    [InlineData("Media")]
    [InlineData("Baixa")]
    public async Task Handle_DeveAceitarTodasAsPrioridades(string prioridade)
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new AlterarPrioridadeChamadoCommand(chamadoId, prioridade);
        
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_DeveRejeitarPrioridadeInvalida()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new AlterarPrioridadeChamadoCommand(chamadoId, "Criticíssima");
        
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_NaoDeveAlterarPrioridadeDeChamadoFechado()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());
        chamado.Atribuir(Guid.NewGuid(), "Victor");
        chamado.Resolver();
        chamado.Fechar();

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new AlterarPrioridadeChamadoCommand(chamadoId, "Urgente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_DeveRegistrarHistoricoComPrioridadeAnteriorENova()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid(), PrioridadeChamado.Media);

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new AlterarPrioridadeChamadoCommand(chamadoId, "Urgente");
        await _handler.Handle(command, CancellationToken.None);

        _historicoRepositoryMock.Verify(
            r => r.AdicionarAsync(
                It.Is<HistoricoEntrada>(h => 
                    h.Acao == AcaoHistorico.PrioridadeAlterada &&
                    h.DetalheAnterior == "Media" &&
                    h.DetalheNovo == "Urgente"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoChamadoNaoExiste_DeveLancarNotFoundException()
    {
        var chamadoId = Guid.NewGuid();
        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado?)null);

        var command = new AlterarPrioridadeChamadoCommand(chamadoId, "Urgente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
