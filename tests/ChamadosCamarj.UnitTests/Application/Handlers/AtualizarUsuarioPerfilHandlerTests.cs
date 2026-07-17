using ChamadosCamarj.Application.Features.Usuarios.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class AtualizarUsuarioPerfilHandlerTests
{
    private readonly Mock<IUsuarioPerfilRepository> _repositoryMock = new();
    private readonly AtualizarUsuarioPerfilCommandHandler _handler;

    public AtualizarUsuarioPerfilHandlerTests()
    {
        _handler = new AtualizarUsuarioPerfilCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioNaoExiste_DeveRetornarNull()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var command = new AtualizarUsuarioPerfilCommand(id, "Novo Nome", Perfil.Admin, true);
        var response = await _handler.Handle(command, CancellationToken.None);

        response.Should().BeNull();
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioPerfil>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveAtualizarNomeEPerfil()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        _repositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new AtualizarUsuarioPerfilCommand(usuario.Id, "Fábio Silva", Perfil.Admin, true);
        var response = await _handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response!.Nome.Should().Be("Fábio Silva");
        response.Perfil.Should().Be(Perfil.Admin);
        _repositoryMock.Verify(r => r.AtualizarAsync(usuario, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComAtivoFalse_DeveDesativarUsuario()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        _repositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new AtualizarUsuarioPerfilCommand(usuario.Id, "Fábio", Perfil.Atendente, false);
        var response = await _handler.Handle(command, CancellationToken.None);

        response!.Ativo.Should().BeFalse();
        usuario.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ComAtivoTrueParaUsuarioDesativado_DeveReativar()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.Desativar();
        _repositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new AtualizarUsuarioPerfilCommand(usuario.Id, "Fábio", Perfil.Atendente, true);
        var response = await _handler.Handle(command, CancellationToken.None);

        response!.Ativo.Should().BeTrue();
        usuario.Ativo.Should().BeTrue();
    }
}
