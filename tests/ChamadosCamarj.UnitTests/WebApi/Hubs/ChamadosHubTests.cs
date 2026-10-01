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
    public async Task OnConnectedAsync_SoAtendimentoEntraNoGrupoDeAlertas(string perfil, bool entraNoAtendimento)
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

        var hub = new ChamadosHub { Groups = groupsMock.Object, Context = contextMock.Object };
        await hub.OnConnectedAsync();

        grupos.Should().Contain("Todos");
        grupos.Contains(ChamadosHub.GrupoAtendimento).Should().Be(entraNoAtendimento);
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
