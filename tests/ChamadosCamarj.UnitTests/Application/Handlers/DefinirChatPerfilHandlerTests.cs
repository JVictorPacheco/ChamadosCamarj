using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.Commands.DefinirChatPerfil;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class DefinirChatPerfilHandlerTests
{
    private readonly Mock<IUsuarioPerfilRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IChatHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IChatConversaRepository> _conversaRepositoryMock = new();
    private readonly Mock<IChatMensagemRepository> _mensagemRepositoryMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly DefinirChatPerfilCommandHandler _handler;

    public DefinirChatPerfilHandlerTests()
    {
        _handler = new DefinirChatPerfilCommandHandler(
            _usuarioRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _conversaRepositoryMock.Object,
            _mensagemRepositoryMock.Object,
            _mediatorMock.Object);
    }

    private UsuarioPerfil CriarUsuario(ChatPerfil chatPerfilAtual)
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.DefinirChatPerfil(chatPerfilAtual);
        return usuario;
    }

    [Fact]
    public async Task Handle_QuandoRequisitanteNaoEhAdmin_DeveLancarForbiddenException()
    {
        var command = new DefinirChatPerfilCommand(Guid.NewGuid(), ChatPerfil.Participante, PerfilRequisitante: "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();

        _usuarioRepositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioNaoExiste_DeveLancarNotFoundException()
    {
        var id = Guid.NewGuid();
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var command = new DefinirChatPerfilCommand(id, ChatPerfil.Participante, PerfilRequisitante: "Admin");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoPerfilNaoMuda_NaoDeveGerarHistoricoNemNotificacao()
    {
        var usuario = CriarUsuario(ChatPerfil.Participante);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new DefinirChatPerfilCommand(usuario.Id, ChatPerfil.Participante, PerfilRequisitante: "Admin");
        await _handler.Handle(command, CancellationToken.None);

        _usuarioRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioPerfil>(), It.IsAny<CancellationToken>()), Times.Never);
        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<ChatHistorico>(), It.IsAny<CancellationToken>()), Times.Never);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AoConcederAcesso_DeveAtualizarUsuarioERegistrarHistoricoComAcessoConcedido()
    {
        var usuario = CriarUsuario(ChatPerfil.SemAcesso);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ListarConversasComUsuarioAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ChatConversa>());

        var command = new DefinirChatPerfilCommand(usuario.Id, ChatPerfil.Participante, "Admin", Guid.NewGuid(), "Admin Requisitante");
        await _handler.Handle(command, CancellationToken.None);

        usuario.ChatPerfil.Should().Be(ChatPerfil.Participante);
        _usuarioRepositoryMock.Verify(r => r.AtualizarAsync(usuario, It.IsAny<CancellationToken>()), Times.Once);

        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatHistorico>(h => h.Acao == ChatAcao.AcessoConcedido),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // Uma troca lateral (já tinha acesso, só muda de nível) não é "restauração" — nunca perdeu
    // conversa nenhuma, não faz sentido anunciar "acesso restaurado" pros outros participantes.
    [Fact]
    public async Task Handle_QuandoJaTinhaAcessoEApenasTrocaDeNivel_NaoDeveCriarMensagemSistemaNemNotificarRevogacao()
    {
        var usuario = CriarUsuario(ChatPerfil.Participante);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new DefinirChatPerfilCommand(usuario.Id, ChatPerfil.CriadorDeGrupo, PerfilRequisitante: "Admin");
        await _handler.Handle(command, CancellationToken.None);

        _mensagemRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<ChatMensagem>(), It.IsAny<CancellationToken>()), Times.Never);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<ChatAcessoRevogadoNotification>(), It.IsAny<CancellationToken>()), Times.Never);
        _conversaRepositoryMock.Verify(r => r.ListarConversasComUsuarioAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // AC-46: restaurar acesso (vindo de SemAcesso) é simétrico a revogar — mensagem de sistema em
    // cada conversa onde a pessoa já era participante (os vínculos nunca foram removidos).
    [Fact]
    public async Task Handle_AoRestaurarAcesso_DeveCriarMensagemSistemaEmCadaConversaAtivaEPublicarEmTempoReal()
    {
        var usuario = CriarUsuario(ChatPerfil.SemAcesso);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var conversa1 = ChatConversa.CriarPrivada(usuario.Id);
        var conversa2 = ChatConversa.CriarGrupo("Grupo Teste", Guid.NewGuid());
        _conversaRepositoryMock.Setup(r => r.ListarConversasComUsuarioAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { conversa1, conversa2 });

        var command = new DefinirChatPerfilCommand(usuario.Id, ChatPerfil.Participante, PerfilRequisitante: "Admin");
        await _handler.Handle(command, CancellationToken.None);

        _mensagemRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatMensagem>(m => m.Tipo == ChatMensagemTipo.Sistema
                && m.Conteudo != null
                && m.Conteudo.Contains(usuario.Nome)
                && m.Conteudo.Contains("restaurado")),
            It.IsAny<CancellationToken>()), Times.Exactly(2));

        _mediatorMock.Verify(m => m.Publish(
            It.Is<ChatNovaMensagemNotification>(n => n.ConversaId == conversa1.Id),
            It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(
            It.Is<ChatNovaMensagemNotification>(n => n.ConversaId == conversa2.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // Restaurar não é revogar — não deve disparar o evento específico de revogação.
    [Fact]
    public async Task Handle_AoRestaurarAcesso_NaoDevePublicarChatAcessoRevogadoNotification()
    {
        var usuario = CriarUsuario(ChatPerfil.SemAcesso);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ListarConversasComUsuarioAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ChatConversa>());

        var command = new DefinirChatPerfilCommand(usuario.Id, ChatPerfil.Participante, PerfilRequisitante: "Admin");
        await _handler.Handle(command, CancellationToken.None);

        _mediatorMock.Verify(m => m.Publish(It.IsAny<ChatAcessoRevogadoNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // AC-47: qualquer mudança de ChatPerfil (concessão, revogação ou restauração) publica o evento
    // global — é isso que permite o frontend atualizar o perfil sem precisar relogar (AC-48).
    [Theory]
    [InlineData(ChatPerfil.SemAcesso, ChatPerfil.Participante)]
    [InlineData(ChatPerfil.Participante, ChatPerfil.SemAcesso)]
    [InlineData(ChatPerfil.Participante, ChatPerfil.CriadorDeGrupo)]
    public async Task Handle_QuandoChatPerfilMuda_DevePublicarChatPerfilAtualizadoComONovoValor(ChatPerfil anterior, ChatPerfil novo)
    {
        var usuario = CriarUsuario(anterior);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ListarConversasComUsuarioAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ChatConversa>());

        var command = new DefinirChatPerfilCommand(usuario.Id, novo, PerfilRequisitante: "Admin");
        await _handler.Handle(command, CancellationToken.None);

        _mediatorMock.Verify(m => m.Publish(
            It.Is<ChatPerfilAtualizadoNotification>(n => n.UsuarioId == usuario.Id && n.NovoChatPerfil == novo),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AoRevogarAcesso_DeveRegistrarHistoricoComAcessoRevogado()
    {
        var usuario = CriarUsuario(ChatPerfil.Participante);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ListarConversasComUsuarioAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ChatConversa>());

        var command = new DefinirChatPerfilCommand(usuario.Id, ChatPerfil.SemAcesso, PerfilRequisitante: "Admin");
        await _handler.Handle(command, CancellationToken.None);

        usuario.ChatPerfil.Should().Be(ChatPerfil.SemAcesso);
        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatHistorico>(h => h.Acao == ChatAcao.AcessoRevogado),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // AC-03: os demais participantes das conversas ativas do usuário revogado devem ver uma
    // mensagem de sistema — e, desde a correção do Bug #8b, isso precisa acontecer em tempo real
    // (ChatNovaMensagemNotification), não só ficar salvo no banco esperando um reload.
    [Fact]
    public async Task Handle_AoRevogarAcesso_DeveCriarMensagemSistemaEmCadaConversaAtivaEPublicarEmTempoReal()
    {
        var usuario = CriarUsuario(ChatPerfil.Participante);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var conversa1 = ChatConversa.CriarPrivada(usuario.Id);
        var conversa2 = ChatConversa.CriarGrupo("Grupo Teste", Guid.NewGuid());
        _conversaRepositoryMock.Setup(r => r.ListarConversasComUsuarioAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { conversa1, conversa2 });

        var command = new DefinirChatPerfilCommand(usuario.Id, ChatPerfil.SemAcesso, PerfilRequisitante: "Admin");
        await _handler.Handle(command, CancellationToken.None);

        _mensagemRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatMensagem>(m => m.Tipo == ChatMensagemTipo.Sistema && m.Conteudo != null && m.Conteudo.Contains(usuario.Nome)),
            It.IsAny<CancellationToken>()), Times.Exactly(2));

        _mediatorMock.Verify(m => m.Publish(
            It.Is<ChatNovaMensagemNotification>(n => n.ConversaId == conversa1.Id),
            It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(
            It.Is<ChatNovaMensagemNotification>(n => n.ConversaId == conversa2.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AoRevogarAcesso_DevePublicarNotificacaoParaOUsuarioRevogado()
    {
        var usuario = CriarUsuario(ChatPerfil.Participante);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ListarConversasComUsuarioAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ChatConversa>());

        var command = new DefinirChatPerfilCommand(usuario.Id, ChatPerfil.SemAcesso, PerfilRequisitante: "Admin");
        await _handler.Handle(command, CancellationToken.None);

        _mediatorMock.Verify(m => m.Publish(
            It.Is<ChatAcessoRevogadoNotification>(n => n.UsuarioId == usuario.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioJaEstaSemAcessoENovoPerfilTambemESemAcesso_NaoFazNada()
    {
        var usuario = CriarUsuario(ChatPerfil.SemAcesso);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new DefinirChatPerfilCommand(usuario.Id, ChatPerfil.SemAcesso, PerfilRequisitante: "Admin");
        await _handler.Handle(command, CancellationToken.None);

        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<ChatHistorico>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
