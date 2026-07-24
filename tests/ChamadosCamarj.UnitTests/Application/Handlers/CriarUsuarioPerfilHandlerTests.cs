using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Usuarios.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class CriarUsuarioPerfilHandlerTests
{
    private readonly Mock<IUsuarioPerfilRepository> _repositoryMock = new();
    private readonly Mock<IPasswordHasher<UsuarioPerfil>> _passwordHasherMock = new();
    private readonly CriarUsuarioPerfilCommandHandler _handler;

    public CriarUsuarioPerfilHandlerTests()
    {
        _passwordHasherMock
            .Setup(h => h.HashPassword(It.IsAny<UsuarioPerfil>(), It.IsAny<string>()))
            .Returns("hash-fake");

        _handler = new CriarUsuarioPerfilCommandHandler(_repositoryMock.Object, _passwordHasherMock.Object);
    }

    [Fact]
    public async Task Handle_ComEmailNovo_DeveCriarUsuario()
    {
        _repositoryMock.Setup(r => r.ObterPorEmailAsync("catia@camarj.com.br", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var command = new CriarUsuarioPerfilCommand("catia@camarj.com.br", "Cátia", Perfil.Solicitante, "SenhaForte123", "Admin");
        var response = await _handler.Handle(command, CancellationToken.None);

        response.Email.Should().Be("catia@camarj.com.br");
        response.Nome.Should().Be("Cátia");
        response.Perfil.Should().Be(Perfil.Solicitante);
        response.Ativo.Should().BeTrue();

        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<UsuarioPerfil>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComEmailJaAtivo_DeveLancarConflictException()
    {
        var existente = new UsuarioPerfil("victor@camarj.com.br", "Victor", Perfil.Admin);
        _repositoryMock.Setup(r => r.ObterPorEmailAsync("victor@camarj.com.br", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existente);

        var command = new CriarUsuarioPerfilCommand("victor@camarj.com.br", "Victor Duplicado", Perfil.Admin, "SenhaForte123", "Admin");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ConflictException>();

        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<UsuarioPerfil>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComEmailDeUsuarioDesativado_DeveReativarRegistroExistente()
    {
        var existenteInativo = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        existenteInativo.Desativar();

        _repositoryMock.Setup(r => r.ObterPorEmailAsync("fabio@camarj.com.br", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existenteInativo);

        var command = new CriarUsuarioPerfilCommand("fabio@camarj.com.br", "Fábio Novo", Perfil.Atendente, "SenhaForte123", "Admin");
        var response = await _handler.Handle(command, CancellationToken.None);

        response.Nome.Should().Be("Fábio Novo");
        response.Ativo.Should().BeTrue();

        // Reativa o registro existente em vez de inserir um novo — o índice único de Email
        // não distingue ativo/inativo, então um Adicionar duplicado quebraria no banco real.
        _repositoryMock.Verify(r => r.AtualizarAsync(existenteInativo, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<UsuarioPerfil>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveNormalizarEmailAntesDeVerificarDuplicidade()
    {
        _repositoryMock.Setup(r => r.ObterPorEmailAsync("catia@camarj.com.br", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var command = new CriarUsuarioPerfilCommand("  Catia@Camarj.COM.BR  ", "Cátia", Perfil.Solicitante, "SenhaForte123", "Admin");
        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(r => r.ObterPorEmailAsync("catia@camarj.com.br", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoRequisitanteNaoEhAdmin_DeveLancarForbiddenException()
    {
        var command = new CriarUsuarioPerfilCommand("catia@camarj.com.br", "Cátia", Perfil.Solicitante, "SenhaForte123", "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();

        _repositoryMock.Verify(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoRequisitanteNulo_DeveLancarForbiddenException()
    {
        var command = new CriarUsuarioPerfilCommand("catia@camarj.com.br", "Cátia", Perfil.Solicitante, "SenhaForte123");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
