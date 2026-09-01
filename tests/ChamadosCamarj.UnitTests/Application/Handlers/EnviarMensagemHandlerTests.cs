using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.Commands.EnviarMensagem;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class EnviarMensagemHandlerTests
{
    private readonly Mock<IChatConversaRepository> _conversaRepositoryMock = new();
    private readonly Mock<IChatMensagemRepository> _mensagemRepositoryMock = new();
    private readonly Mock<IChatHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IUsuarioPerfilRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly EnviarMensagemCommandHandler _handler;

    public EnviarMensagemHandlerTests()
    {
        _handler = new EnviarMensagemCommandHandler(
            _conversaRepositoryMock.Object,
            _mensagemRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _usuarioRepositoryMock.Object,
            _mediatorMock.Object);
    }

    private UsuarioPerfil CriarUsuarioComAcesso(ChatPerfil chatPerfil = ChatPerfil.Participante)
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.DefinirChatPerfil(chatPerfil);
        return usuario;
    }

    [Fact]
    public async Task Handle_QuandoUsuarioNaoExiste_DeveLancarNotFoundException()
    {
        var usuarioId = Guid.NewGuid();
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var command = new EnviarMensagemCommand(Guid.NewGuid(), "Olá", UsuarioId: usuarioId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoUsuarioSemAcessoAoChat_DeveLancarForbiddenException()
    {
        var usuario = CriarUsuarioComAcesso(ChatPerfil.SemAcesso);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new EnviarMensagemCommand(Guid.NewGuid(), "Olá", UsuarioId: usuario.Id);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();

        _conversaRepositoryMock.Verify(r => r.ObterParticipanteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoNaoEhParticipanteDaConversa_DeveLancarForbiddenException()
    {
        var usuario = CriarUsuarioComAcesso();
        var conversaId = Guid.NewGuid();
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ObterParticipanteAsync(conversaId, usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatParticipante?)null);

        var command = new EnviarMensagemCommand(conversaId, "Olá", UsuarioId: usuario.Id);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_QuandoParticipanteFoiDesativado_DeveLancarForbiddenException()
    {
        var usuario = CriarUsuarioComAcesso();
        var conversaId = Guid.NewGuid();
        var participante = new ChatParticipante(conversaId, usuario.Id, usuario.Nome);
        participante.Desativar();

        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ObterParticipanteAsync(conversaId, usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participante);

        var command = new EnviarMensagemCommand(conversaId, "Olá", UsuarioId: usuario.Id);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_ComMensagemValida_DeveEnviarRegistrarHistoricoEPublicarNotificacao()
    {
        var usuario = CriarUsuarioComAcesso();
        var conversaId = Guid.NewGuid();
        var participante = new ChatParticipante(conversaId, usuario.Id, usuario.Nome);

        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ObterParticipanteAsync(conversaId, usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participante);

        var command = new EnviarMensagemCommand(conversaId, "Olá, tudo bem?", UsuarioId: usuario.Id, UsuarioNome: usuario.Nome);
        var response = await _handler.Handle(command, CancellationToken.None);

        response.Conteudo.Should().Be("Olá, tudo bem?");
        response.AutorId.Should().Be(usuario.Id);

        _mensagemRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<ChatMensagem>(), It.IsAny<CancellationToken>()), Times.Once);
        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatHistorico>(h => h.Acao == ChatAcao.MensagemEnviada),
            It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(
            It.Is<ChatNovaMensagemNotification>(n => n.ConversaId == conversaId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoRespostaParaMensagemNaoExiste_DeveLancarNotFoundException()
    {
        var usuario = CriarUsuarioComAcesso();
        var conversaId = Guid.NewGuid();
        var participante = new ChatParticipante(conversaId, usuario.Id, usuario.Nome);
        var mensagemCitadaId = Guid.NewGuid();

        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ObterParticipanteAsync(conversaId, usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participante);
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagemCitadaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatMensagem?)null);

        var command = new EnviarMensagemCommand(conversaId, "Respondendo", mensagemCitadaId, usuario.Id);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoMensagemCitadaEhDeOutraConversa_DeveLancarBadRequestException()
    {
        var usuario = CriarUsuarioComAcesso();
        var conversaId = Guid.NewGuid();
        var participante = new ChatParticipante(conversaId, usuario.Id, usuario.Nome);
        var mensagemDeOutraConversa = ChatMensagem.CriarTexto(Guid.NewGuid(), usuario.Id, usuario.Nome, "original");

        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ObterParticipanteAsync(conversaId, usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participante);
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagemDeOutraConversa.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagemDeOutraConversa);

        var command = new EnviarMensagemCommand(conversaId, "Respondendo", mensagemDeOutraConversa.Id, usuario.Id);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ComResposta_DevePopularRespostaConteudoComOTextoDaMensagemOriginal()
    {
        var usuario = CriarUsuarioComAcesso();
        var conversaId = Guid.NewGuid();
        var participante = new ChatParticipante(conversaId, usuario.Id, usuario.Nome);
        var original = ChatMensagem.CriarTexto(conversaId, usuario.Id, usuario.Nome, "mensagem original");

        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ObterParticipanteAsync(conversaId, usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participante);
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(original.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(original);

        var command = new EnviarMensagemCommand(conversaId, "Respondendo", original.Id, usuario.Id);
        var response = await _handler.Handle(command, CancellationToken.None);

        response.RespostaParaMensagemId.Should().Be(original.Id);
        response.RespostaConteudo.Should().Be("mensagem original");
    }
}
