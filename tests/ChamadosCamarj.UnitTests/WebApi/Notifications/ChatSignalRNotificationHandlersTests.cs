using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.WebApi.Hubs;
using ChamadosCamarj.WebApi.Notifications;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace ChamadosCamarj.UnitTests.WebApi.Notifications;

// review-fase9-independente.md #4: os handlers de SignalR concentram a lógica de "quem recebe o
// quê" (achado #2 vivia exatamente aqui) e não tinham nenhum teste — WebApi ficava fora do alcance
// do projeto de testes. Referência adicionada em ChamadosCamarj.UnitTests.csproj destrava isto.
public class ChatSignalRNotificationHandlersTests
{
    private readonly Mock<IHubContext<ChatHub>> _chatHubContextMock = new();
    private readonly Mock<IClientProxy> _chatGroupProxyMock = new();
    private readonly Mock<IHubContext<ChamadosHub>> _chamadosHubContextMock = new();
    private readonly Mock<IClientProxy> _chamadosUserProxyMock = new();
    private readonly Mock<IChatConversaRepository> _conversaRepositoryMock = new();
    private readonly Mock<IUsuarioPerfilRepository> _usuarioPerfilRepositoryMock = new();
    private readonly ChatNovaMensagemNotificationHandler _handler;

    private readonly List<string> _usuariosNotificados = [];

    public ChatSignalRNotificationHandlersTests()
    {
        var chatClientsMock = new Mock<IHubClients>();
        chatClientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(_chatGroupProxyMock.Object);
        _chatHubContextMock.Setup(h => h.Clients).Returns(chatClientsMock.Object);
        _chatGroupProxyMock
            .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var chamadosClientsMock = new Mock<IHubClients>();
        // Devolve o mesmo proxy pra qualquer usuário, mas registra o id pedido — é o suficiente
        // pra verificar quem foi notificado sem precisar de um mock por usuário.
        chamadosClientsMock.Setup(c => c.User(It.IsAny<string>()))
            .Returns((string id) => { _usuariosNotificados.Add(id); return _chamadosUserProxyMock.Object; });
        _chamadosHubContextMock.Setup(h => h.Clients).Returns(chamadosClientsMock.Object);
        _chamadosUserProxyMock
            .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new ChatNovaMensagemNotificationHandler(
            _chatHubContextMock.Object,
            _chamadosHubContextMock.Object,
            _conversaRepositoryMock.Object,
            _usuarioPerfilRepositoryMock.Object,
            Mock.Of<ILogger<ChatNovaMensagemNotificationHandler>>());
    }

    private static UsuarioPerfil CriarUsuario(ChatPerfil chatPerfil)
    {
        var usuario = new UsuarioPerfil($"{Guid.NewGuid()}@camarj.com.br", "Teste", Perfil.Atendente);
        usuario.DefinirChatPerfil(chatPerfil);
        return usuario;
    }

    [Fact]
    public async Task Handle_DeveExcluirRemetenteEUsuarioComChatPerfilSemAcesso()
    {
        var autor = CriarUsuario(ChatPerfil.Participante);
        var comAcesso = CriarUsuario(ChatPerfil.Participante);
        var semAcesso = CriarUsuario(ChatPerfil.SemAcesso);

        var conversa = ChatConversa.CriarGrupo("Equipe", autor.Id);
        conversa.AdicionarParticipante(new ChatParticipante(conversa.Id, autor.Id, autor.Nome));
        conversa.AdicionarParticipante(new ChatParticipante(conversa.Id, comAcesso.Id, comAcesso.Nome));
        conversa.AdicionarParticipante(new ChatParticipante(conversa.Id, semAcesso.Id, semAcesso.Nome));

        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(conversa.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversa);
        _usuarioPerfilRepositoryMock
            .Setup(r => r.ListarPorIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UsuarioPerfil> { comAcesso, semAcesso });

        var mensagem = new ChatMensagemResponse(
            Guid.NewGuid(), conversa.Id, autor.Id, autor.Nome, "oi", ChatMensagemTipo.Texto,
            false, null, null, null, null, null, null, [], DateTime.UtcNow);

        await _handler.Handle(new ChatNovaMensagemNotification(conversa.Id, mensagem), CancellationToken.None);

        _usuariosNotificados.Should().ContainSingle().Which.Should().Be(comAcesso.Id.ToString());
    }

    [Fact]
    public async Task Handle_QuandoDestinatarioIdsFornecido_NaoDeveConsultarRepositorioDeConversa()
    {
        var autor = CriarUsuario(ChatPerfil.Participante);
        var destinatario = CriarUsuario(ChatPerfil.Participante);
        var conversaId = Guid.NewGuid();

        _usuarioPerfilRepositoryMock
            .Setup(r => r.ListarPorIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UsuarioPerfil> { destinatario });

        var mensagem = new ChatMensagemResponse(
            Guid.NewGuid(), conversaId, autor.Id, autor.Nome, null, ChatMensagemTipo.Sistema,
            false, null, null, null, null, null, null, [], DateTime.UtcNow);

        await _handler.Handle(
            new ChatNovaMensagemNotification(conversaId, mensagem, [autor.Id, destinatario.Id]),
            CancellationToken.None);

        _conversaRepositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _usuariosNotificados.Should().ContainSingle().Which.Should().Be(destinatario.Id.ToString());
    }

    [Fact]
    public async Task Handle_QuandoConversaNaoExiste_NaoDeveLancarNemNotificarNinguem()
    {
        var conversaId = Guid.NewGuid();
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(conversaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatConversa?)null);

        var mensagem = new ChatMensagemResponse(
            Guid.NewGuid(), conversaId, Guid.NewGuid(), "Autor", "oi", ChatMensagemTipo.Texto,
            false, null, null, null, null, null, null, [], DateTime.UtcNow);

        var act = async () => await _handler.Handle(new ChatNovaMensagemNotification(conversaId, mensagem), CancellationToken.None);

        await act.Should().NotThrowAsync();
        _usuariosNotificados.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_QuandoRepositorioDeUsuariosLancaExcecao_NaoDevePropagar()
    {
        // Achado #3 (robustez): a mensagem já foi persistida antes deste handler rodar — uma falha
        // aqui não pode subir e derrubar o comando de envio que já teve sucesso.
        var autor = Guid.NewGuid();
        var conversa = ChatConversa.CriarGrupo("Equipe", autor);
        conversa.AdicionarParticipante(new ChatParticipante(conversa.Id, autor, "Autor"));
        conversa.AdicionarParticipante(new ChatParticipante(conversa.Id, Guid.NewGuid(), "Outro"));

        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(conversa.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversa);
        _usuarioPerfilRepositoryMock
            .Setup(r => r.ListarPorIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("banco fora do ar"));

        var mensagem = new ChatMensagemResponse(
            Guid.NewGuid(), conversa.Id, autor, "Autor", "oi", ChatMensagemTipo.Texto,
            false, null, null, null, null, null, null, [], DateTime.UtcNow);

        var act = async () => await _handler.Handle(new ChatNovaMensagemNotification(conversa.Id, mensagem), CancellationToken.None);

        await act.Should().NotThrowAsync();
        // O aviso pro ChatHub (grupo da conversa, quem está com a tela aberta) acontece antes do
        // bloco protegido e não deve ser afetado pela falha no fan-out do ChamadosHub.
        _chatGroupProxyMock.Verify(p => p.SendCoreAsync("NovaMensagem", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoMensagemNaoEhChatMensagemResponse_NaoDeveFazerFanOut()
    {
        var act = async () => await _handler.Handle(
            new ChatNovaMensagemNotification(Guid.NewGuid(), new { texto = "algo" }),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
        _usuariosNotificados.Should().BeEmpty();
        _conversaRepositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class ChatPerfilAtualizadoNotificationHandlerTests
{
    [Fact]
    public async Task Handle_DeveNotificarOUsuarioAfetadoPeloChamadosHub()
    {
        var usuarioId = Guid.NewGuid();
        var usuariosNotificados = new List<string>();

        var proxyMock = new Mock<IClientProxy>();
        proxyMock.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var clientsMock = new Mock<IHubClients>();
        clientsMock.Setup(c => c.User(It.IsAny<string>()))
            .Returns((string id) => { usuariosNotificados.Add(id); return proxyMock.Object; });

        var hubContextMock = new Mock<IHubContext<ChamadosHub>>();
        hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        var handler = new ChatPerfilAtualizadoNotificationHandler(hubContextMock.Object);

        await handler.Handle(new ChatPerfilAtualizadoNotification(usuarioId, ChatPerfil.Participante), CancellationToken.None);

        usuariosNotificados.Should().ContainSingle().Which.Should().Be(usuarioId.ToString());
    }
}
