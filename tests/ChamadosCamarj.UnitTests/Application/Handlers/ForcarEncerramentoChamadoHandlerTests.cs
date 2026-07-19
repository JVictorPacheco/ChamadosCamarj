using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ForcarEncerramentoChamadoHandlerTests
{
    private readonly Mock<IChamadoRepository> _chamadoRepositoryMock = new();
    private readonly Mock<IHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly ForcarEncerramentoChamadoCommandHandler _handler;

    public ForcarEncerramentoChamadoHandlerTests()
    {
        _handler = new ForcarEncerramentoChamadoCommandHandler(
            _chamadoRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _publisherMock.Object);
    }

    private static Chamado CriarChamado() => new("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());

    [Fact]
    public async Task Handle_ComoAdmin_DeveFecharChamadoAbertoERegistrarHistorico()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = CriarChamado();

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new ForcarEncerramentoChamadoCommand(chamadoId, "Chamado duplicado, aberto por engano.", Guid.NewGuid(), "Victor", "Admin");
        await _handler.Handle(command, CancellationToken.None);

        chamado.Status.Should().Be(StatusChamado.Fechado);

        _chamadoRepositoryMock.Verify(r => r.AtualizarAsync(chamado, It.IsAny<CancellationToken>()), Times.Once);
        _historicoRepositoryMock.Verify(
            r => r.AdicionarAsync(
                It.Is<HistoricoEntrada>(h =>
                    h.Acao == AcaoHistorico.EncerramentoForcado &&
                    h.DetalheAnterior == "Aberto" &&
                    h.DetalheNovo == "Chamado duplicado, aberto por engano."),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ComMotivoComEspacosNasPontas_DeveGravarHistoricoAparado()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = CriarChamado();

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new ForcarEncerramentoChamadoCommand(chamadoId, "   Motivo com espaços nas pontas.   ", PerfilRequisitante: "Admin");
        await _handler.Handle(command, CancellationToken.None);

        _historicoRepositoryMock.Verify(
            r => r.AdicionarAsync(
                It.Is<HistoricoEntrada>(h => h.DetalheNovo == "Motivo com espaços nas pontas."),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ComoAtendente_DeveLancarForbiddenException()
    {
        var chamadoId = Guid.NewGuid();
        var command = new ForcarEncerramentoChamadoCommand(chamadoId, "Motivo válido com mais de dez caracteres.", Guid.NewGuid(), "Fábio", "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();

        _chamadoRepositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SemPerfilRequisitante_DeveLancarForbiddenException()
    {
        var command = new ForcarEncerramentoChamadoCommand(Guid.NewGuid(), "Motivo válido com mais de dez caracteres.");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_QuandoChamadoNaoExiste_DeveLancarNotFoundException()
    {
        var chamadoId = Guid.NewGuid();
        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado?)null);

        var command = new ForcarEncerramentoChamadoCommand(chamadoId, "Motivo válido com mais de dez caracteres.", PerfilRequisitante: "Admin");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoChamadoJaFechado_DeveLancarInvalidOperationExceptionENaoRegistrarHistorico()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = CriarChamado();
        chamado.Atribuir(Guid.NewGuid(), "Victor");
        chamado.Resolver();
        chamado.Fechar();

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new ForcarEncerramentoChamadoCommand(chamadoId, "Motivo válido com mais de dez caracteres.", PerfilRequisitante: "Admin");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<HistoricoEntrada>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeResolvido_DevePreservarDataConclusaoNoHistoricoDetalheAnterior()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = CriarChamado();
        chamado.Atribuir(Guid.NewGuid(), "Victor");
        chamado.Resolver();
        var dataConclusaoOriginal = chamado.DataConclusao;

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new ForcarEncerramentoChamadoCommand(chamadoId, "Encerrando manualmente após validação.", PerfilRequisitante: "Admin");
        await _handler.Handle(command, CancellationToken.None);

        chamado.Status.Should().Be(StatusChamado.Fechado);
        chamado.DataConclusao.Should().Be(dataConclusaoOriginal);

        _historicoRepositoryMock.Verify(
            r => r.AdicionarAsync(
                It.Is<HistoricoEntrada>(h => h.DetalheAnterior == "Resolvido"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
