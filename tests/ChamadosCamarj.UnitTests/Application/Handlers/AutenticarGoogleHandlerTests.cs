using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Auth.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class AutenticarGoogleHandlerTests
{
    private readonly Mock<IUsuarioPerfilRepository> _repositoryMock = new();
    private readonly Mock<IGoogleTokenValidator> _googleValidatorMock = new();
    private readonly AutenticarGoogleCommandHandler _handler;

    public AutenticarGoogleHandlerTests()
    {
        var authSettings = Options.Create(new AuthSettings
        {
            GoogleClientId = "client-id-de-teste",
            JwtSigningKey = "chave-de-teste-com-pelo-menos-32-caracteres-para-hmac-sha256",
            TokenExpiracaoHoras = 10,
        });

        _handler = new AutenticarGoogleCommandHandler(_repositoryMock.Object, _googleValidatorMock.Object, authSettings);
    }

    private static GoogleJsonWebSignature.Payload CriarPayload(string email, bool emailVerified = true) =>
        new() { Email = email, EmailVerified = emailVerified, Name = "Usuário Teste" };

    [Fact]
    public async Task Handle_ComTokenInvalido_DeveLancarUnauthorizedException()
    {
        _googleValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidJwtException("token inválido"));

        var act = async () => await _handler.Handle(new AutenticarGoogleCommand("token-qualquer"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_ComEmailNaoVerificado_DeveLancarUnauthorizedException()
    {
        _googleValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(CriarPayload("victor@camarj.com.br", emailVerified: false));

        var act = async () => await _handler.Handle(new AutenticarGoogleCommand("token"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _repositoryMock.Verify(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComDominioForaDeCamarj_DeveLancarUnauthorizedException()
    {
        _googleValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(CriarPayload("alguem@gmail.com"));

        var act = async () => await _handler.Handle(new AutenticarGoogleCommand("token"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _repositoryMock.Verify(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComEmailNaoCadastrado_DeveLancarForbiddenException()
    {
        _googleValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(CriarPayload("naoexiste@camarj.com.br"));

        _repositoryMock
            .Setup(r => r.ObterPorEmailAsync("naoexiste@camarj.com.br", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var act = async () => await _handler.Handle(new AutenticarGoogleCommand("token"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_ComUsuarioInativo_DeveLancarForbiddenException()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.Desativar();

        _googleValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(CriarPayload("fabio@camarj.com.br"));

        _repositoryMock
            .Setup(r => r.ObterPorEmailAsync("fabio@camarj.com.br", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var act = async () => await _handler.Handle(new AutenticarGoogleCommand("token"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_ComUsuarioValidoEAtivo_DeveEmitirTokenComOsClaimsCorretos()
    {
        var usuario = new UsuarioPerfil("victor@camarj.com.br", "Victor", Perfil.Admin);

        _googleValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(CriarPayload("victor@camarj.com.br"));

        _repositoryMock
            .Setup(r => r.ObterPorEmailAsync("victor@camarj.com.br", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var resultado = await _handler.Handle(new AutenticarGoogleCommand("token"), CancellationToken.None);

        resultado.Token.Should().NotBeNullOrWhiteSpace();
        resultado.Id.Should().Be(usuario.Id);
        resultado.Nome.Should().Be("Victor");
        resultado.Email.Should().Be("victor@camarj.com.br");
        resultado.Perfil.Should().Be(Perfil.Admin);

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(resultado.Token);
        jwt.Claims.Should().Contain(c => c.Type == "perfil" && c.Value == "Admin");
        jwt.Claims.Should().Contain(c => c.Value == "victor@camarj.com.br");
    }

    [Fact]
    public async Task Handle_DeveNormalizarEmailAntesDeBuscarNoRepositorio()
    {
        var usuario = new UsuarioPerfil("catia@camarj.com.br", "Cátia", Perfil.Solicitante);

        _googleValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(CriarPayload("  Catia@Camarj.COM.BR  "));

        _repositoryMock
            .Setup(r => r.ObterPorEmailAsync("catia@camarj.com.br", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        await _handler.Handle(new AutenticarGoogleCommand("token"), CancellationToken.None);

        _repositoryMock.Verify(r => r.ObterPorEmailAsync("catia@camarj.com.br", It.IsAny<CancellationToken>()), Times.Once);
    }
}
