using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.WebApi.Hubs;
using ChamadosCamarj.WebApi.Notifications;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace ChamadosCamarj.UnitTests.WebApi.Notifications;

/// <summary>spec controle-de-acesso AC-11/AC-13: o aviso de acessos vai só para a pessoa afetada.</summary>
public class AcessoSignalRNotificationHandlersTests
{
    [Fact]
    public async Task AcessosAtualizados_VaiSoParaOUsuario_ComOsModulosEOPerfil()
    {
        var usuarioId = Guid.NewGuid();
        string? destino = null;
        string? evento = null;
        object? payload = null;
        var proxyMock = new Mock<IClientProxy>();
        proxyMock.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Callback<string, object?[], CancellationToken>((e, args, _) => { evento = e; payload = args[0]; })
            .Returns(Task.CompletedTask);
        var clientsMock = new Mock<IHubClients>(MockBehavior.Strict);
        clientsMock.Setup(c => c.User(It.IsAny<string>()))
            .Callback<string>(u => destino = u)
            .Returns(proxyMock.Object);
        var hubMock = new Mock<IHubContext<ChamadosHub>>();
        hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        await new AcessosAtualizadosNotificationHandler(hubMock.Object).Handle(
            new AcessosAtualizadosNotification(usuarioId, ["Arquivo"], ChatPerfil.SemAcesso, Perfil.Solicitante), CancellationToken.None);

        destino.Should().Be(usuarioId.ToString());
        evento.Should().Be("AcessosAtualizados");
        // review-2 R-03: a tela compara este perfil com o da sessão para saber se precisa sair.
        payload.Should().BeEquivalentTo(new { modulos = new[] { "Arquivo" }, chatPerfil = "SemAcesso", perfil = "Solicitante" });
    }
}
