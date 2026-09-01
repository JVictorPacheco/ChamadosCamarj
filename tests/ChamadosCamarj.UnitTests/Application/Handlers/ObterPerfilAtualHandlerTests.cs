using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Auth.Queries;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

// AC-48 / review-fase9-independente.md #10.
public class ObterPerfilAtualHandlerTests
{
    private readonly Mock<IUsuarioPerfilRepository> _repositoryMock = new();
    private readonly ObterPerfilAtualQueryHandler _handler;

    public ObterPerfilAtualHandlerTests()
    {
        _handler = new ObterPerfilAtualQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioExisteEAtivo_DeveRetornarPerfilAtualDoBanco()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.DefinirChatPerfil(ChatPerfil.SemAcesso);
        _repositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var resultado = await _handler.Handle(new ObterPerfilAtualQuery(usuario.Id), CancellationToken.None);

        // Confirma que reflete o valor ATUAL do banco, não um snapshot antigo — é exatamente o que
        // fecha a lacuna do AC-48 pra quem foi revogado enquanto estava deslogado.
        resultado.ChatPerfil.Should().Be(ChatPerfil.SemAcesso);
        resultado.Id.Should().Be(usuario.Id);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioNaoExiste_DeveLancarUnauthorizedException()
    {
        var usuarioId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ObterPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var act = async () => await _handler.Handle(new ObterPerfilAtualQuery(usuarioId), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_QuandoUsuarioDesativado_DeveLancarUnauthorizedException()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.Desativar();
        _repositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var act = async () => await _handler.Handle(new ObterPerfilAtualQuery(usuario.Id), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
