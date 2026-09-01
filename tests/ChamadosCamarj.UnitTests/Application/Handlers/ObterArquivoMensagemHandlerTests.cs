using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chat.Queries.ObterArquivoMensagem;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

// review-fase9-independente.md #2 — ver mesmo comentário em ListarConversasHandlerTests.cs.
public class ObterArquivoMensagemHandlerTests
{
    private readonly Mock<IChatMensagemRepository> _mensagemRepositoryMock = new();
    private readonly Mock<IChatConversaRepository> _conversaRepositoryMock = new();
    private readonly Mock<IChatStorageService> _storageServiceMock = new();
    private readonly Mock<IUsuarioPerfilRepository> _usuarioPerfilRepositoryMock = new();
    private readonly ObterArquivoMensagemQueryHandler _handler;

    public ObterArquivoMensagemHandlerTests()
    {
        _handler = new ObterArquivoMensagemQueryHandler(
            _mensagemRepositoryMock.Object,
            _conversaRepositoryMock.Object,
            _storageServiceMock.Object,
            _usuarioPerfilRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioSemAcesso_DeveLancarForbiddenException()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var query = new ObterArquivoMensagemQuery(Guid.NewGuid(), usuario.Id);
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _mensagemRepositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioNaoExiste_DeveLancarNotFoundException()
    {
        var usuarioId = Guid.NewGuid();
        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var query = new ObterArquivoMensagemQuery(Guid.NewGuid(), usuarioId);
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoUsuarioComAcessoEArquivoValido_DeveRetornarUrlAssinada()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.DefinirChatPerfil(ChatPerfil.Participante);

        var conversa = ChatConversa.CriarPrivada(usuario.Id);
        var mensagem = ChatMensagem.CriarArquivo(conversa.Id, usuario.Id, usuario.Nome, "foto.png", "chat/x/foto.png", "image/png", 1024);
        var participante = new ChatParticipante(conversa.Id, usuario.Id, usuario.Nome);

        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);
        _conversaRepositoryMock.Setup(r => r.ObterParticipanteAsync(conversa.Id, usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participante);
        _storageServiceMock.Setup(s => s.ObterUrlAssinadaAsync("chat/x/foto.png", It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.example/assinada");

        var query = new ObterArquivoMensagemQuery(mensagem.Id, usuario.Id);
        var resultado = await _handler.Handle(query, CancellationToken.None);

        resultado.UrlAssinada.Should().Be("https://storage.example/assinada");
    }
}
