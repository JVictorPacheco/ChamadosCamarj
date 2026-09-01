using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chat.Queries.ListarConversas;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

// review-fase9-independente.md #2: nenhuma query de leitura do chat verificava ChatPerfil, só
// participação ativa — revogar acesso nunca bloqueou a leitura via API. Estes testes cobrem
// especificamente a guarda nova (ChatPerfilGuard.ExigirAcesso), não o comportamento pré-existente
// do handler (que já funcionava e não tinha teste nenhum antes desta correção).
public class ListarConversasHandlerTests
{
    private readonly Mock<IChatConversaRepository> _conversaRepositoryMock = new();
    private readonly Mock<IChatMensagemRepository> _mensagemRepositoryMock = new();
    private readonly Mock<IUsuarioPerfilRepository> _usuarioPerfilRepositoryMock = new();
    private readonly ListarConversasQueryHandler _handler;

    public ListarConversasHandlerTests()
    {
        _handler = new ListarConversasQueryHandler(
            _conversaRepositoryMock.Object,
            _mensagemRepositoryMock.Object,
            _usuarioPerfilRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioSemAcesso_DeveLancarForbiddenException()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var query = new ListarConversasQuery(usuario.Id);
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _conversaRepositoryMock.Verify(r => r.ListarPorUsuarioAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioNaoExiste_DeveLancarNotFoundException()
    {
        var usuarioId = Guid.NewGuid();
        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var query = new ListarConversasQuery(usuarioId);
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoUsuarioTemAcesso_DeveListarConversas()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.DefinirChatPerfil(ChatPerfil.Participante);
        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var conversa = ChatConversa.CriarPrivada(usuario.Id);
        _conversaRepositoryMock.Setup(r => r.ListarPorUsuarioAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatConversa> { conversa });
        _mensagemRepositoryMock.Setup(r => r.ObterUltimasMensagensPorConversasAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, ChatMensagem>());
        _mensagemRepositoryMock.Setup(r => r.ContarNaoLidasPorConversasAsync(
                It.IsAny<IEnumerable<(Guid ConversaId, DateTime? UltimaLeituraEm)>>(), usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        var query = new ListarConversasQuery(usuario.Id);
        var resultado = await _handler.Handle(query, CancellationToken.None);

        resultado.Should().ContainSingle(c => c.Id == conversa.Id);
    }
}
