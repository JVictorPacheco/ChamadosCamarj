using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.WebApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ChamadosCamarj.UnitTests.WebApi.Services;

/// <summary>
/// spec perfil-no-cadastro AC-02, AC-06..08 (T9): do token antigo até a regra de permissão. Usa o mesmo
/// caminho do servidor — validador → CurrentUserService → contexto de acesso → ChamadoPermissoes — para
/// provar que a regra enxerga o cadastro, e não o perfil/equipe do dia do login.
/// </summary>
public class PerfilNoCadastroNasRegrasDeAcessoTests
{
    private readonly Guid _id = Guid.NewGuid();
    private readonly DadosDoChamado _chamadoDeOutraPessoa = new("outra@camarj.com.br", null, StatusChamado.Aberto);

    private async Task<ContextoAcesso> ContextoAposOPedidoAsync(string perfilDoToken, IdentidadeUsuario cadastro, Guid? equipeDoToken = null)
    {
        var usuarios = new Mock<IUsuarioPerfilRepository>();
        usuarios.Setup(u => u.ObterIdentidadeAsync(_id, It.IsAny<CancellationToken>())).ReturnsAsync(cadastro);

        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, _id.ToString()), new("perfil", perfilDoToken), new(ClaimTypes.Email, "pessoa@camarj.com.br") };
        if (equipeDoToken.HasValue)
            claims.Add(new Claim("grupo_id", equipeDoToken.Value.ToString()));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

        await new CadastroClaimsValidator(usuarios.Object).ValidarEAtualizarAsync(principal, default);

        var http = new Mock<IHttpContextAccessor>();
        http.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = principal });
        return new CurrentUserService(http.Object).ObterContextoAcesso();
    }

    [Theory]
    [InlineData(AcaoChamado.Assumir)]
    [InlineData(AcaoChamado.Resolver)]
    [InlineData(AcaoChamado.Encerrar)]
    [InlineData(AcaoChamado.ComentarInterno)]
    [InlineData(AcaoChamado.Reatribuir)]
    public async Task AtendenteRebaixadoParaSolicitante_NaoPodeMaisAgirComoAtendente(AcaoChamado acao)
    {
        var contexto = await ContextoAposOPedidoAsync("Atendente", new IdentidadeUsuario(Perfil.Solicitante, true, null));

        ChamadoPermissoes.Pode(acao, contexto, _chamadoDeOutraPessoa).Should().BeFalse();
    }

    [Theory]
    [InlineData(AcaoChamado.AlterarPrioridade)]
    [InlineData(AcaoChamado.ForcarEncerramento)]
    [InlineData(AcaoChamado.Reatribuir)]
    public async Task AdminRebaixadoParaAtendente_NaoPodeMaisAgirComoAdmin(AcaoChamado acao)
    {
        var contexto = await ContextoAposOPedidoAsync("Admin", new IdentidadeUsuario(Perfil.Atendente, true, null));

        ChamadoPermissoes.Pode(acao, contexto, _chamadoDeOutraPessoa).Should().BeFalse();
    }

    [Fact]
    public async Task SolicitantePromovidoParaAtendente_PassaAPoderAssumirSemRelogar()
    {
        var contexto = await ContextoAposOPedidoAsync("Solicitante", new IdentidadeUsuario(Perfil.Atendente, true, null));

        ChamadoPermissoes.Pode(AcaoChamado.Assumir, contexto, _chamadoDeOutraPessoa).Should().BeTrue();
    }

    [Fact]
    public async Task EquipeTrocada_ContextoDeAcessoCarregaAEquipeDoCadastro()
    {
        var antiga = Guid.NewGuid();
        var nova = Guid.NewGuid();

        var contexto = await ContextoAposOPedidoAsync("Atendente", new IdentidadeUsuario(Perfil.Atendente, true, nova), antiga);

        contexto.GrupoId.Should().Be(nova); // é este valor que o filtro de visibilidade usa
    }

    [Fact]
    public async Task EquipeRemovida_ContextoDeAcessoFicaSemEquipe()
    {
        var contexto = await ContextoAposOPedidoAsync("Atendente", new IdentidadeUsuario(Perfil.Atendente, true, null), Guid.NewGuid());

        contexto.GrupoId.Should().BeNull();
    }
}
