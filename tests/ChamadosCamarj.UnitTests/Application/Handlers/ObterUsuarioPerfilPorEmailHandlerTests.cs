using ChamadosCamarj.Application.Features.Usuarios.Queries;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ObterUsuarioPerfilPorEmailHandlerTests
{
    private readonly Mock<IUsuarioPerfilRepository> _repositoryMock = new();
    private readonly ObterUsuarioPerfilPorEmailQueryHandler _handler;

    public ObterUsuarioPerfilPorEmailHandlerTests()
    {
        _handler = new ObterUsuarioPerfilPorEmailQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ComUsuarioAtivoExistente_DeveRetornarResponse()
    {
        var usuario = new UsuarioPerfil("victor@camarj.com.br", "Victor", Perfil.Admin);
        _repositoryMock.Setup(r => r.ObterPorEmailAsync("victor@camarj.com.br", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var resultado = await _handler.Handle(new ObterUsuarioPerfilPorEmailQuery("victor@camarj.com.br"), CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.Email.Should().Be("victor@camarj.com.br");
        resultado.Perfil.Should().Be(Perfil.Admin);
    }

    [Fact]
    public async Task Handle_ComEmailNaoCadastrado_DeveRetornarNull()
    {
        _repositoryMock.Setup(r => r.ObterPorEmailAsync("desconhecido@camarj.com.br", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var resultado = await _handler.Handle(new ObterUsuarioPerfilPorEmailQuery("desconhecido@camarj.com.br"), CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ComUsuarioDesativado_DeveRetornarNull()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.Desativar();

        _repositoryMock.Setup(r => r.ObterPorEmailAsync("fabio@camarj.com.br", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var resultado = await _handler.Handle(new ObterUsuarioPerfilPorEmailQuery("fabio@camarj.com.br"), CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeveNormalizarEmailAntesDeBuscar()
    {
        var usuario = new UsuarioPerfil("victor@camarj.com.br", "Victor", Perfil.Admin);
        _repositoryMock.Setup(r => r.ObterPorEmailAsync("victor@camarj.com.br", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var resultado = await _handler.Handle(new ObterUsuarioPerfilPorEmailQuery("  Victor@Camarj.COM.BR  "), CancellationToken.None);

        resultado.Should().NotBeNull();
        _repositoryMock.Verify(r => r.ObterPorEmailAsync("victor@camarj.com.br", It.IsAny<CancellationToken>()), Times.Once);
    }
}
