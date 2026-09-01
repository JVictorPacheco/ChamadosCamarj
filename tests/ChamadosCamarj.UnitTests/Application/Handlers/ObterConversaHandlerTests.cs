using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chat.Queries.ObterConversa;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

// review-fase9-independente.md #2 — ver mesmo comentário em ListarConversasHandlerTests.cs.
public class ObterConversaHandlerTests
{
    private readonly Mock<IChatConversaRepository> _conversaRepositoryMock = new();
    private readonly Mock<IUsuarioPerfilRepository> _usuarioPerfilRepositoryMock = new();
    private readonly ObterConversaQueryHandler _handler;

    public ObterConversaHandlerTests()
    {
        _handler = new ObterConversaQueryHandler(_conversaRepositoryMock.Object, _usuarioPerfilRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioSemAcesso_DeveLancarForbiddenException()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var query = new ObterConversaQuery(Guid.NewGuid(), usuario.Id);
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _conversaRepositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioComAcessoRevogadoContinuaParticipante_AindaAssimDeveBloquear()
    {
        // Confirma exatamente o cenário do achado #2: revogar não remove o vínculo de participante,
        // então sem a guarda por ChatPerfil essa pessoa continuaria conseguindo ler a conversa.
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        // ChatPerfil já nasce SemAcesso — não precisa conceder e revogar, só confirma o estado.

        var conversa = ChatConversa.CriarGrupo("Equipe", Guid.NewGuid());
        conversa.AdicionarParticipante(new ChatParticipante(conversa.Id, usuario.Id, usuario.Nome));

        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(conversa.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversa);

        var query = new ObterConversaQuery(conversa.Id, usuario.Id);
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_QuandoUsuarioNaoExiste_DeveLancarNotFoundException()
    {
        var usuarioId = Guid.NewGuid();
        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var query = new ObterConversaQuery(Guid.NewGuid(), usuarioId);
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoUsuarioComAcessoEParticipante_DeveRetornarDetalhe()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.DefinirChatPerfil(ChatPerfil.Participante);

        var conversa = ChatConversa.CriarGrupo("Equipe", Guid.NewGuid());
        conversa.AdicionarParticipante(new ChatParticipante(conversa.Id, usuario.Id, usuario.Nome));

        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(conversa.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversa);

        var query = new ObterConversaQuery(conversa.Id, usuario.Id);
        var resultado = await _handler.Handle(query, CancellationToken.None);

        resultado.Id.Should().Be(conversa.Id);
    }
}
