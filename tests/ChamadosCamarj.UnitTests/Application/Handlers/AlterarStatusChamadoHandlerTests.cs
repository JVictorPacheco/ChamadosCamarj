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

public class AlterarStatusChamadoHandlerTests
{
    private readonly Mock<IChamadoRepository> _chamadoRepositoryMock = new();
    private readonly Mock<IHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly AlterarStatusChamadoCommandHandler _handler;

    public AlterarStatusChamadoHandlerTests()
    {
        _handler = new AlterarStatusChamadoCommandHandler(
            _chamadoRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _publisherMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_DeveAlterarStatusEPersistir()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new AlterarStatusChamadoCommand(chamadoId, StatusChamado.EmAndamento, Guid.NewGuid(), "Fábio");
        await _handler.Handle(command, CancellationToken.None);

        chamado.Status.Should().Be(StatusChamado.EmAndamento);
        _chamadoRepositoryMock.Verify(r => r.AtualizarAsync(chamado, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRegistrarHistoricoComAutorEStatusAnteriorENovo()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());
        var usuarioId = Guid.NewGuid();

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new AlterarStatusChamadoCommand(chamadoId, StatusChamado.EmAndamento, usuarioId, "Fábio");
        await _handler.Handle(command, CancellationToken.None);

        _historicoRepositoryMock.Verify(
            r => r.AdicionarAsync(
                It.Is<HistoricoEntrada>(h =>
                    h.Acao == AcaoHistorico.StatusAlterado &&
                    h.DetalheAnterior == "Aberto" &&
                    h.DetalheNovo == "EmAndamento" &&
                    h.UsuarioId == usuarioId &&
                    h.UsuarioNome == "Fábio"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SemAtor_DeveRegistrarHistoricoComoSistema()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var command = new AlterarStatusChamadoCommand(chamadoId, StatusChamado.EmAndamento);
        await _handler.Handle(command, CancellationToken.None);

        _historicoRepositoryMock.Verify(
            r => r.AdicionarAsync(
                It.Is<HistoricoEntrada>(h => h.UsuarioNome == "Sistema" && h.UsuarioId == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoChamadoNaoExiste_DeveLancarNotFoundException()
    {
        var chamadoId = Guid.NewGuid();
        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado?)null);

        var command = new AlterarStatusChamadoCommand(chamadoId, StatusChamado.EmAndamento);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();

        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<HistoricoEntrada>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComTransicaoInvalida_NaoDeveRegistrarHistorico()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        // Chamado está Aberto — não pode ir direto para Fechado
        var command = new AlterarStatusChamadoCommand(chamadoId, StatusChamado.Fechado);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<HistoricoEntrada>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
