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
    public async Task AcessosAtualizados_VaiSoParaOUsuario_ComOsModulos()
    {
        var usuarioId = Guid.NewGuid();
        string? destino = null;
        string? evento = null;
        var proxyMock = new Mock<IClientProxy>();
        proxyMock.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Callback<string, object?[], CancellationToken>((e, _, _) => evento = e)
            .Returns(Task.CompletedTask);
        var clientsMock = new Mock<IHubClients>(MockBehavior.Strict);
        clientsMock.Setup(c => c.User(It.IsAny<string>()))
            .Callback<string>(u => destino = u)
            .Returns(proxyMock.Object);
        var hubMock = new Mock<IHubContext<ChamadosHub>>();
        hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        await new AcessosAtualizadosNotificationHandler(hubMock.Object).Handle(
            new AcessosAtualizadosNotification(usuarioId, ["Arquivo"], ChatPerfil.SemAcesso), CancellationToken.None);

        destino.Should().Be(usuarioId.ToString());
        evento.Should().Be("AcessosAtualizados");
    }
}
