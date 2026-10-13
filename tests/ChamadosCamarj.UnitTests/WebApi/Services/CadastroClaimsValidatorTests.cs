using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.WebApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ChamadosCamarj.UnitTests.WebApi.Services;

/// <summary>
/// spec perfil-no-cadastro AC-01..04, 06..08, 10, 11, 16: o perfil e a equipe do token valem só até o
/// próximo pedido; depois vale o cadastro.
/// </summary>
public class CadastroClaimsValidatorTests
{
    private readonly Mock<IUsuarioPerfilRepository> _usuarios = new();
    private readonly CadastroClaimsValidator _validador;
    private readonly Guid _id = Guid.NewGuid();

    public CadastroClaimsValidatorTests() => _validador = new CadastroClaimsValidator(_usuarios.Object);

    private ClaimsPrincipal Token(string perfil, Guid? grupo = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, _id.ToString()),
            new("perfil", perfil),
        };
        if (grupo.HasValue)
            claims.Add(new Claim("grupo_id", grupo.Value.ToString()));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
    }

    private void Cadastro(Perfil perfil, bool ativo = true, Guid? grupo = null) =>
        _usuarios.Setup(u => u.ObterIdentidadeAsync(_id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentidadeUsuario(perfil, ativo, grupo));

    [Fact]
    public async Task PerfilRebaixado_ClaimPassaAValerOCadastro()
    {
        Cadastro(Perfil.Solicitante);
        var principal = Token("Admin");

        (await _validador.ValidarEAtualizarAsync(principal, default)).Should().BeTrue();

        principal.FindFirstValue("perfil").Should().Be("Solicitante");
    }

    [Fact]
    public async Task PerfilPromovido_ClaimPassaAValerOCadastro()
    {
        Cadastro(Perfil.Atendente);
        var principal = Token("Solicitante");

        await _validador.ValidarEAtualizarAsync(principal, default);

        principal.FindFirstValue("perfil").Should().Be("Atendente");
    }

    [Fact]
    public async Task EquipeTrocada_ClaimPassaAValerOCadastro()
    {
        var antiga = Guid.NewGuid();
        var nova = Guid.NewGuid();
        Cadastro(Perfil.Atendente, grupo: nova);
        var principal = Token("Atendente", antiga);

        await _validador.ValidarEAtualizarAsync(principal, default);

        principal.FindAll("grupo_id").Should().ContainSingle().Which.Value.Should().Be(nova.ToString());
    }

    [Fact]
    public async Task EquipeRemovida_ClaimDeEquipeSai()
    {
        Cadastro(Perfil.Atendente, grupo: null);
        var principal = Token("Atendente", Guid.NewGuid());

        await _validador.ValidarEAtualizarAsync(principal, default);

        principal.FindFirst("grupo_id").Should().BeNull();
    }

    [Fact]
    public async Task EquipeColocada_ClaimDeEquipeEntra()
    {
        var nova = Guid.NewGuid();
        Cadastro(Perfil.Atendente, grupo: nova);
        var principal = Token("Atendente");

        await _validador.ValidarEAtualizarAsync(principal, default);

        principal.FindFirstValue("grupo_id").Should().Be(nova.ToString());
    }

    [Fact]
    public async Task ContaDesativada_RecusaOToken()
    {
        Cadastro(Perfil.Atendente, ativo: false);

        (await _validador.ValidarEAtualizarAsync(Token("Atendente"), default)).Should().BeFalse();
    }

    [Fact]
    public async Task ContaApagada_RecusaOToken()
    {
        _usuarios.Setup(u => u.ObterIdentidadeAsync(_id, It.IsAny<CancellationToken>())).ReturnsAsync((IdentidadeUsuario?)null);

        (await _validador.ValidarEAtualizarAsync(Token("Atendente"), default)).Should().BeFalse();
    }

    [Fact]
    public async Task SemMudancaNoCadastro_ClaimsContinuamIguais()
    {
        var equipe = Guid.NewGuid();
        Cadastro(Perfil.Atendente, grupo: equipe);
        var principal = Token("Atendente", equipe);

        (await _validador.ValidarEAtualizarAsync(principal, default)).Should().BeTrue();

        principal.FindFirstValue("perfil").Should().Be("Atendente");
        principal.FindAll("grupo_id").Should().ContainSingle().Which.Value.Should().Be(equipe.ToString());
        principal.FindFirstValue(JwtRegisteredClaimNames.Sub).Should().Be(_id.ToString());
    }

    [Fact]
    public async Task TokenSemIdValido_Recusa()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("perfil", "Admin")], "jwt"));

        (await _validador.ValidarEAtualizarAsync(principal, default)).Should().BeFalse();
        _usuarios.Verify(u => u.ObterIdentidadeAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BancoIndisponivel_NaoAceitaOTokenSemConferir()
    {
        _usuarios.Setup(u => u.ObterIdentidadeAsync(_id, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("banco fora"));

        var act = () => _validador.ValidarEAtualizarAsync(Token("Admin"), default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task PerfilAtualizadoChegaNoCurrentUserENoContextoDeAcesso()
    {
        // O resto do servidor só lê estes claims: confirma que o valor novo chega nas regras de visibilidade.
        var equipe = Guid.NewGuid();
        Cadastro(Perfil.Solicitante, grupo: equipe);
        var principal = Token("Admin");
        await _validador.ValidarEAtualizarAsync(principal, default);

        var http = new Mock<IHttpContextAccessor>();
        http.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = principal });
        var contexto = new CurrentUserService(http.Object).ObterContextoAcesso();

        contexto.Perfil.Should().Be(Perfil.Solicitante);
        contexto.GrupoId.Should().Be(equipe);
    }
}
