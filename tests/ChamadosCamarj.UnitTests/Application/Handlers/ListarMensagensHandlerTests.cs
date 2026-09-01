using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chat.Queries.ListarMensagens;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

// review-fase9-independente.md #2 — ver mesmo comentário em ListarConversasHandlerTests.cs.
public class ListarMensagensHandlerTests
{
    private readonly Mock<IChatConversaRepository> _conversaRepositoryMock = new();
    private readonly Mock<IChatMensagemRepository> _mensagemRepositoryMock = new();
    private readonly Mock<IUsuarioPerfilRepository> _usuarioPerfilRepositoryMock = new();
    private readonly ListarMensagensQueryHandler _handler;

    public ListarMensagensHandlerTests()
    {
        _handler = new ListarMensagensQueryHandler(
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

        var query = new ListarMensagensQuery(Guid.NewGuid(), UsuarioId: usuario.Id);
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _conversaRepositoryMock.Verify(r => r.ObterParticipanteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioComAcessoMasNaoParticipaDaConversa_DeveLancarForbiddenException()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.DefinirChatPerfil(ChatPerfil.Participante);
        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var conversaId = Guid.NewGuid();
        _conversaRepositoryMock.Setup(r => r.ObterParticipanteAsync(conversaId, usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatParticipante?)null);

        var query = new ListarMensagensQuery(conversaId, UsuarioId: usuario.Id);
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_QuandoUsuarioNaoExiste_DeveLancarNotFoundException()
    {
        var usuarioId = Guid.NewGuid();
        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var query = new ListarMensagensQuery(Guid.NewGuid(), UsuarioId: usuarioId);
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
