using ChamadosCamarj.Application.Features.Usuarios.Queries;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ListarUsuariosPerfilHandlerTests
{
    private readonly Mock<IUsuarioPerfilRepository> _repositoryMock = new();
    private readonly ListarUsuariosPerfilQueryHandler _handler;

    public ListarUsuariosPerfilHandlerTests()
    {
        _handler = new ListarUsuariosPerfilQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarUsuariosOrdenadosPorNome()
    {
        var vitor = new UsuarioPerfil("victor@camarj.com.br", "Victor", Perfil.Admin);
        var fabio = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);

        _repositoryMock.Setup(r => r.ListarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UsuarioPerfil> { vitor, fabio });

        var resultado = (await _handler.Handle(new ListarUsuariosPerfilQuery(), CancellationToken.None)).ToList();

        resultado.Should().HaveCount(2);
        resultado[0].Nome.Should().Be("Fábio");
        resultado[1].Nome.Should().Be("Victor");
    }

    [Fact]
    public async Task Handle_SemUsuarios_DeveRetornarListaVazia()
    {
        _repositoryMock.Setup(r => r.ListarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UsuarioPerfil>());

        var resultado = await _handler.Handle(new ListarUsuariosPerfilQuery(), CancellationToken.None);

        resultado.Should().BeEmpty();
    }
}
