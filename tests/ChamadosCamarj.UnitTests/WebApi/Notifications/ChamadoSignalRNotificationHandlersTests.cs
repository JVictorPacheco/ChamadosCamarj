using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.WebApi.Hubs;
using ChamadosCamarj.WebApi.Notifications;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace ChamadosCamarj.UnitTests.WebApi.Notifications;

/// <summary>
/// Quem recebe o aviso de comentário novo — spec correcoes-acesso-chamados (review R-01): o aviso
/// de um comentário interno não pode chegar a quem não pode ver comentários internos.
/// </summary>
public class ChamadoSignalRNotificationHandlersTests
{
    private readonly List<string> _gruposAvisados = [];
    private readonly ComentarioAdicionadoNotificationHandler _handler;

    public ChamadoSignalRNotificationHandlersTests()
    {
        var proxyMock = new Mock<IClientProxy>();
        proxyMock.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var clientsMock = new Mock<IHubClients>();
        clientsMock.Setup(c => c.Group(It.IsAny<string>()))
            .Callback<string>(g => _gruposAvisados.Add(g))
            .Returns(proxyMock.Object);
        var hubMock = new Mock<IHubContext<ChamadosHub>>();
        hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
        _handler = new ComentarioAdicionadoNotificationHandler(hubMock.Object);
    }

    [Fact]
    public async Task ComentarioPublico_AvisaTodos()
    {
        await _handler.Handle(new ComentarioAdicionadoNotification(Guid.NewGuid(), "Ana", "texto"), CancellationToken.None);

        _gruposAvisados.Should().Equal("Todos");
    }

    [Fact]
    public async Task ComentarioInterno_AvisaSoOAtendimento()
    {
        await _handler.Handle(new ComentarioAdicionadoNotification(Guid.NewGuid(), "Atendente", "texto", Interno: true), CancellationToken.None);

        _gruposAvisados.Should().Equal(ChamadosHub.GrupoAtendimento);
    }
}
