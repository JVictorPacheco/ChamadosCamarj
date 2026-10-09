using System.Reflection;
using System.Security.Claims;
using ChamadosCamarj.WebApi.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace ChamadosCamarj.UnitTests.WebApi.Hubs;

/// <summary>
/// spec correcoes-acesso-chamados AC-03/AC-04: só Atendente/Admin entram no grupo que recebe os
/// alertas de SLA, e o cliente não tem como escolher grupo por conta própria.
/// </summary>
public class ChamadosHubTests
{
    private static ClaimsPrincipal Usuario(string? perfil) =>
        new(new ClaimsIdentity(perfil is null ? [] : [new Claim("perfil", perfil)], "jwt"));

    [Theory]
    [InlineData("Atendente", true)]
    [InlineData("Admin", true)]
    [InlineData("admin", true)]
    [InlineData("Solicitante", false)]
    [InlineData(null, false)]
    public void EhAtendimento_SoAtendenteEAdmin(string? perfil, bool esperado)
    {
        ChamadosHub.EhAtendimento(Usuario(perfil)).Should().Be(esperado);
    }

    [Fact]
    public void EhAtendimento_SemUsuario_False()
    {
        ChamadosHub.EhAtendimento(null).Should().BeFalse();
    }

    [Theory]
    [InlineData("Atendente", true)]
    [InlineData("Admin", true)]
    [InlineData("Solicitante", false)]
    public async Task OnConnectedAsync_SoAtendimentoEntraNoGrupoDeAtendimento(string perfil, bool entraNoAtendimento)
    {
        // review R-04: sem este teste, apagar o if do OnConnectedAsync deixaria a suíte verde.
        var grupos = new List<string>();
        var groupsMock = new Mock<IGroupManager>();
        groupsMock.Setup(g => g.AddToGroupAsync("conn-1", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, grupo, _) => grupos.Add(grupo))
            .Returns(Task.CompletedTask);
        var contextMock = new Mock<HubCallerContext>();
        contextMock.SetupGet(c => c.ConnectionId).Returns("conn-1");
        contextMock.SetupGet(c => c.User).Returns(Usuario(perfil));

        var hub = new ChamadosHub(new ConexoesTempoReal()) { Groups = groupsMock.Object, Context = contextMock.Object };
        await hub.OnConnectedAsync();

        grupos.Should().Contain("Todos");
        grupos.Contains(ChamadosHub.GrupoAtendimento).Should().Be(entraNoAtendimento);
    }

    // spec correcoes-pre-deploy AC-03/AC-04: só Admin entra no grupo que recebe todos os alertas de SLA.
    [Theory]
    [InlineData("Admin", true)]
    [InlineData("Atendente", false)]
    [InlineData("Solicitante", false)]
    public async Task OnConnectedAsync_SoAdminEntraNoGrupoAdmins(string perfil, bool entraEmAdmins)
    {
        var grupos = new List<string>();
        var groupsMock = new Mock<IGroupManager>();
        groupsMock.Setup(g => g.AddToGroupAsync("conn-1", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, grupo, _) => grupos.Add(grupo))
            .Returns(Task.CompletedTask);
        var contextMock = new Mock<HubCallerContext>();
        contextMock.SetupGet(c => c.ConnectionId).Returns("conn-1");
        contextMock.SetupGet(c => c.User).Returns(Usuario(perfil));

        var hub = new ChamadosHub(new ConexoesTempoReal()) { Groups = groupsMock.Object, Context = contextMock.Object };
        await hub.OnConnectedAsync();

        grupos.Contains(ChamadosHub.GrupoAdmins).Should().Be(entraEmAdmins);
    }

    // spec perfil-no-cadastro D7: a mesma regra de grupos serve à conexão nova e ao reajuste de quem já está conectado.
    [Theory]
    [InlineData("Admin", new[] { "Atendimento", "Admins" })]
    [InlineData("Atendente", new[] { "Atendimento" })]
    [InlineData("Solicitante", new string[0])]
    [InlineData(null, new string[0])]
    public void GruposDoPerfil_SegueOPerfil(string? perfil, string[] esperado)
    {
        ChamadosHub.GruposDoPerfil(perfil).Should().BeEquivalentTo(esperado);
    }

    [Fact]
    public async Task OnConnectedAsync_RegistraAConexaoAntesDeEntrarNosGrupos()
    {
        // Review R-02 de perfil-no-cadastro: uma mudança que chegue durante a entrada nos grupos já acha a conexão.
        var registro = new ConexoesTempoReal();
        var usuario = Guid.NewGuid();
        var registradaAoEntrarNoPrimeiroGrupo = false;
        var groupsMock = new Mock<IGroupManager>();
        groupsMock.Setup(g => g.AddToGroupAsync("conn-1", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, _, _) => registradaAoEntrarNoPrimeiroGrupo |= registro.DoUsuario(usuario).Count == 1)
            .Returns(Task.CompletedTask);
        var contextMock = new Mock<HubCallerContext>();
        contextMock.SetupGet(c => c.ConnectionId).Returns("conn-1");
        contextMock.SetupGet(c => c.UserIdentifier).Returns(usuario.ToString());
        contextMock.SetupGet(c => c.User).Returns(Usuario("Admin"));

        var hub = new ChamadosHub(registro) { Groups = groupsMock.Object, Context = contextMock.Object };
        await hub.OnConnectedAsync();

        registradaAoEntrarNoPrimeiroGrupo.Should().BeTrue();
    }

    [Fact]
    public void Hub_NaoExpoeMetodoParaOClienteEntrarEmGrupo()
    {
        // Métodos públicos de um Hub viram chamadas que qualquer cliente conectado pode invocar.
        var metodosDoCliente = typeof(ChamadosHub)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName)
            .Select(m => m.Name);

        metodosDoCliente.Should().BeEquivalentTo(["OnConnectedAsync", "OnDisconnectedAsync"]);
    }
}
