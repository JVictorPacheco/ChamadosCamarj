using System.Reflection;
using System.Security.Claims;
using ChamadosCamarj.WebApi.Hubs;
using FluentAssertions;

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
