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

public class ForcarEncerramentoChamadoHandlerTests
{
    private readonly Mock<IChamadoRepository> _chamadoRepositoryMock = new();
    private readonly Mock<IHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ForcarEncerramentoChamadoCommandHandler _handler;

    public ForcarEncerramentoChamadoHandlerTests()
    {
        _handler = new ForcarEncerramentoChamadoCommandHandler(
            _chamadoRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _publisherMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Chamado CriarChamado() => new("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());

    [Fact]
    public async Task Handle_ComoAdmin_DeveFecharChamadoAbertoERegistrarHistorico()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = CriarChamado();

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdComTrackingAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new ForcarEncerramentoChamadoCommand(chamadoId, MotivoEncerramento.Duplicata, null, null, Guid.NewGuid(), "Victor", "Admin");
        await _handler.Handle(command, CancellationToken.None);

        chamado.Status.Should().Be(StatusChamado.Fechado);

        _chamadoRepositoryMock.Verify(r => r.AtualizarAsync(chamado, It.IsAny<CancellationToken>()), Times.Once);
        _historicoRepositoryMock.Verify(
            r => r.AdicionarAsync(
                It.Is<HistoricoEntrada>(h =>
                    h.Acao == AcaoHistorico.EncerramentoForcado &&
                    h.DetalheAnterior == "Aberto" &&
                    h.DetalheNovo == "Duplicata"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ComMotivoComEspacosNasPontas_DeveGravarHistoricoAparado()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = CriarChamado();

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdComTrackingAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new ForcarEncerramentoChamadoCommand(chamadoId, MotivoEncerramento.Outro, "Aberto por engano.", PerfilRequisitante: "Admin");
        await _handler.Handle(command, CancellationToken.None);

        _historicoRepositoryMock.Verify(
            r => r.AdicionarAsync(
                It.Is<HistoricoEntrada>(h => h.DetalheNovo == "Outro: Aberto por engano."),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ComoAtendente_DeveLancarForbiddenException()
    {
        var chamadoId = Guid.NewGuid();
        var command = new ForcarEncerramentoChamadoCommand(chamadoId, MotivoEncerramento.AbertoIndevidamente, null, null, Guid.NewGuid(), "Fábio", "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();

        _chamadoRepositoryMock.Verify(r => r.ObterPorIdComTrackingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SemPerfilRequisitante_DeveLancarForbiddenException()
    {
        var command = new ForcarEncerramentoChamadoCommand(Guid.NewGuid(), MotivoEncerramento.AbertoIndevidamente);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_QuandoChamadoNaoExiste_DeveLancarNotFoundException()
    {
        var chamadoId = Guid.NewGuid();
        _chamadoRepositoryMock.Setup(r => r.ObterPorIdComTrackingAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado?)null);

        var command = new ForcarEncerramentoChamadoCommand(chamadoId, MotivoEncerramento.AbertoIndevidamente, null, PerfilRequisitante: "Admin");

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

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdComTrackingAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new ForcarEncerramentoChamadoCommand(chamadoId, MotivoEncerramento.AbertoIndevidamente, null, PerfilRequisitante: "Admin");

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

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdComTrackingAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new ForcarEncerramentoChamadoCommand(chamadoId, MotivoEncerramento.AbertoIndevidamente, null, PerfilRequisitante: "Admin");
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
