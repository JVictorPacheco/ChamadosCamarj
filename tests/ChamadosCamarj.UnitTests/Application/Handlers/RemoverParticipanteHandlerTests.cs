using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.Commands.RemoverParticipante;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class RemoverParticipanteHandlerTests
{
    private readonly Mock<IChatConversaRepository> _conversaRepositoryMock = new();
    private readonly Mock<IChatMensagemRepository> _mensagemRepositoryMock = new();
    private readonly Mock<IChatHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly RemoverParticipanteCommandHandler _handler;

    public RemoverParticipanteHandlerTests()
    {
        _handler = new RemoverParticipanteCommandHandler(
            _conversaRepositoryMock.Object,
            _mensagemRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _mediatorMock.Object);
    }

    private static ChatConversa CriarGrupoComParticipante(Guid criadorId, out ChatParticipante participante)
    {
        var grupo = ChatConversa.CriarGrupo("Grupo", criadorId);
        participante = new ChatParticipante(grupo.Id, Guid.NewGuid(), "Mathes");
        grupo.AdicionarParticipante(participante);
        return grupo;
    }

    [Fact]
    public async Task Handle_QuandoConversaNaoExiste_DeveLancarNotFoundException()
    {
        var conversaId = Guid.NewGuid();
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(conversaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatConversa?)null);

        var command = new RemoverParticipanteCommand(conversaId, Guid.NewGuid());

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoConversaNaoEhGrupo_DeveLancarBadRequestException()
    {
        var criadorId = Guid.NewGuid();
        var conversaPrivada = ChatConversa.CriarPrivada(criadorId);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(conversaPrivada.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversaPrivada);

        var command = new RemoverParticipanteCommand(conversaPrivada.Id, Guid.NewGuid(), criadorId, "Criador", "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_QuandoRequisitanteNaoEhCriadorNemAdmin_DeveLancarForbiddenException()
    {
        var criadorId = Guid.NewGuid();
        var grupo = CriarGrupoComParticipante(criadorId, out var participante);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(grupo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);

        var command = new RemoverParticipanteCommand(grupo.Id, participante.UsuarioId, Guid.NewGuid(), "Outra Pessoa", "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();

        participante.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_QuandoRequisitanteEhAdminMasNaoOCriador_DevePermitirRemover()
    {
        var criadorId = Guid.NewGuid();
        var grupo = CriarGrupoComParticipante(criadorId, out var participante);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(grupo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);

        var command = new RemoverParticipanteCommand(grupo.Id, participante.UsuarioId, Guid.NewGuid(), "Um Admin", "Admin");
        await _handler.Handle(command, CancellationToken.None);

        participante.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_QuandoUsuarioNaoEhParticipante_DeveLancarNotFoundException()
    {
        var criadorId = Guid.NewGuid();
        var grupo = ChatConversa.CriarGrupo("Grupo", criadorId);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(grupo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);

        var command = new RemoverParticipanteCommand(grupo.Id, Guid.NewGuid(), criadorId, "Criador", "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoUsuarioJaEstaInativoNoGrupo_DeveLancarNotFoundException()
    {
        var criadorId = Guid.NewGuid();
        var grupo = CriarGrupoComParticipante(criadorId, out var participante);
        participante.Desativar();
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(grupo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);

        var command = new RemoverParticipanteCommand(grupo.Id, participante.UsuarioId, criadorId, "Criador", "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveDesativarParticipanteRegistrarHistoricoEPublicarNotificacoes()
    {
        var criadorId = Guid.NewGuid();
        var grupo = CriarGrupoComParticipante(criadorId, out var participante);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(grupo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);

        var command = new RemoverParticipanteCommand(grupo.Id, participante.UsuarioId, criadorId, "Criador", "Atendente");
        await _handler.Handle(command, CancellationToken.None);

        participante.Ativo.Should().BeFalse();
        _conversaRepositoryMock.Verify(r => r.AtualizarParticipanteAsync(participante, It.IsAny<CancellationToken>()), Times.Once);

        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatHistorico>(h => h.Acao == ChatAcao.ParticipanteRemovido),
            It.IsAny<CancellationToken>()), Times.Once);

        _mensagemRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatMensagem>(m => m.Tipo == ChatMensagemTipo.Sistema),
            It.IsAny<CancellationToken>()), Times.Once);

        _mediatorMock.Verify(m => m.Publish(It.IsAny<ChatNovaMensagemNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(
            It.Is<ChatParticipanteRemovidoNotification>(n => n.ConversaId == grupo.Id && n.UsuarioId == participante.UsuarioId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
