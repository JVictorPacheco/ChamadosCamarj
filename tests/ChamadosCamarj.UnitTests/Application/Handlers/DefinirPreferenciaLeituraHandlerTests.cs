using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chat.Commands.DefinirPreferenciaLeitura;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

// AC-55 a AC-58: toggle de preferência de leitura, self-service (sem guard de Admin).
public class DefinirPreferenciaLeituraHandlerTests
{
    private readonly Mock<IUsuarioPerfilRepository> _usuarioRepositoryMock = new();
    private readonly DefinirPreferenciaLeituraCommandHandler _handler;

    public DefinirPreferenciaLeituraHandlerTests()
    {
        _handler = new DefinirPreferenciaLeituraCommandHandler(_usuarioRepositoryMock.Object);
    }

    private static UsuarioPerfil CriarUsuario() => new("usuario@camarj.com.br", "Usuário Teste", Perfil.Atendente);

    [Fact]
    public async Task Handle_QuandoUsuarioNaoExiste_DeveLancarNotFoundException()
    {
        var id = Guid.NewGuid();
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((UsuarioPerfil?)null);

        var act = async () => await _handler.Handle(new DefinirPreferenciaLeituraCommand(false, id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_AoDesligarPreferencia_DeveAtualizarUsuario()
    {
        var usuario = CriarUsuario();
        usuario.MostrarConfirmacaoLeitura.Should().BeTrue(); // default

        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);

        await _handler.Handle(new DefinirPreferenciaLeituraCommand(false, usuario.Id), CancellationToken.None);

        usuario.MostrarConfirmacaoLeitura.Should().BeFalse();
        _usuarioRepositoryMock.Verify(r => r.AtualizarAsync(usuario, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AoReligarPreferencia_DeveAtualizarUsuario()
    {
        var usuario = CriarUsuario();
        usuario.DefinirPreferenciaLeitura(false);

        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);

        await _handler.Handle(new DefinirPreferenciaLeituraCommand(true, usuario.Id), CancellationToken.None);

        usuario.MostrarConfirmacaoLeitura.Should().BeTrue();
        _usuarioRepositoryMock.Verify(r => r.AtualizarAsync(usuario, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoValorNaoMuda_NaoDeveChamarAtualizarAsync()
    {
        var usuario = CriarUsuario(); // já começa com MostrarConfirmacaoLeitura = true

        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);

        await _handler.Handle(new DefinirPreferenciaLeituraCommand(true, usuario.Id), CancellationToken.None);

        _usuarioRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioPerfil>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
