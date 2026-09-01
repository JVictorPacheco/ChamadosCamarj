using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.Commands.AtualizarPresenca;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ChatPresencaHandlerTests
{
    private readonly Mock<IChatPresencaRepository> _presencaRepositoryMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly AtualizarPresencaCommandHandler _handler;

    public ChatPresencaHandlerTests()
    {
        _handler = new AtualizarPresencaCommandHandler(_presencaRepositoryMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_ComStatusNulo_DeveTratarComoHeartbeatEMarcarOnline()
    {
        var usuarioId = Guid.NewGuid();
        _presencaRepositoryMock.Setup(r => r.ObterPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatPresenca?)null);

        var command = new AtualizarPresencaCommand(Status: null, UsuarioId: usuarioId, UsuarioNome: "Fábio");
        await _handler.Handle(command, CancellationToken.None);

        _presencaRepositoryMock.Verify(r => r.AdicionarOuAtualizarAsync(
            It.Is<ChatPresenca>(p => p.Status == StatusPresenca.Online),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComStatusExplicito_DeveDefinirOStatusInformado()
    {
        var usuarioId = Guid.NewGuid();
        _presencaRepositoryMock.Setup(r => r.ObterPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatPresenca?)null);

        // AC-09: logout marca Offline imediatamente — não passa pelo caminho de heartbeat.
        var command = new AtualizarPresencaCommand(Status: StatusPresenca.Offline, UsuarioId: usuarioId, UsuarioNome: "Fábio");
        await _handler.Handle(command, CancellationToken.None);

        _presencaRepositoryMock.Verify(r => r.AdicionarOuAtualizarAsync(
            It.Is<ChatPresenca>(p => p.Status == StatusPresenca.Offline),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoPresencaNaoExisteAinda_DeveCriarUmaNova()
    {
        var usuarioId = Guid.NewGuid();
        _presencaRepositoryMock.Setup(r => r.ObterPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatPresenca?)null);

        var command = new AtualizarPresencaCommand(Status: null, UsuarioId: usuarioId, UsuarioNome: "Novo Usuário");
        await _handler.Handle(command, CancellationToken.None);

        _presencaRepositoryMock.Verify(r => r.AdicionarOuAtualizarAsync(
            It.Is<ChatPresenca>(p => p.UsuarioId == usuarioId && p.UsuarioNome == "Novo Usuário"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoPresencaJaExiste_DeveReutilizarARegistroExistenteEmVezDeCriarOutro()
    {
        var usuarioId = Guid.NewGuid();
        var existente = new ChatPresenca(usuarioId, "Fábio");
        _presencaRepositoryMock.Setup(r => r.ObterPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existente);

        var command = new AtualizarPresencaCommand(Status: null, UsuarioId: usuarioId, UsuarioNome: "Fábio");
        await _handler.Handle(command, CancellationToken.None);

        _presencaRepositoryMock.Verify(r => r.AdicionarOuAtualizarAsync(existente, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DevePublicarNotificacaoDePresencaAtualizadaComOStatusFinal()
    {
        var usuarioId = Guid.NewGuid();
        _presencaRepositoryMock.Setup(r => r.ObterPorUsuarioAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatPresenca?)null);

        var command = new AtualizarPresencaCommand(Status: StatusPresenca.Ausente, UsuarioId: usuarioId, UsuarioNome: "Fábio");
        await _handler.Handle(command, CancellationToken.None);

        _mediatorMock.Verify(m => m.Publish(
            It.Is<ChatPresencaAtualizadaNotification>(n =>
                n.UsuarioId == usuarioId && n.UsuarioNome == "Fábio" && n.Status == StatusPresenca.Ausente.ToString()),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
