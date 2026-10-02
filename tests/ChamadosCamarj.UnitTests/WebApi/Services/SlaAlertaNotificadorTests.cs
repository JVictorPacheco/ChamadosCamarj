using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.WebApi.Hubs;
using ChamadosCamarj.WebApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace ChamadosCamarj.UnitTests.WebApi.Services;

/// <summary>
/// Quem recebe o alerta de SLA — spec correcoes-pre-deploy AC-01..04: Admins sempre, Atendentes só
/// se podem ver o chamado (lista vem da regra única do repositório), Solicitante nunca.
/// </summary>
public class SlaAlertaNotificadorTests
{
    private readonly List<string> _grupos = [];
    private readonly List<IReadOnlyList<string>> _usuarios = [];
    private readonly List<string> _eventos = [];
    private readonly Mock<IChamadoRepository> _repoMock = new();
    private readonly SlaAlertaNotificador _notificador;

    public SlaAlertaNotificadorTests()
    {
        var proxyMock = new Mock<IClientProxy>();
        proxyMock.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Callback<string, object?[], CancellationToken>((evento, _, _) => _eventos.Add(evento))
            .Returns(Task.CompletedTask);
        var clientsMock = new Mock<IHubClients>();
        clientsMock.Setup(c => c.Group(It.IsAny<string>()))
            .Callback<string>(g => _grupos.Add(g))
            .Returns(proxyMock.Object);
        clientsMock.Setup(c => c.Users(It.IsAny<IReadOnlyList<string>>()))
            .Callback<IReadOnlyList<string>>(u => _usuarios.Add(u))
            .Returns(proxyMock.Object);
        clientsMock.Setup(c => c.All).Throws(new InvalidOperationException("alerta de SLA não pode ir para todos"));
        var hubMock = new Mock<IHubContext<ChamadosHub>>();
        hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
        _notificador = new SlaAlertaNotificador(hubMock.Object, _repoMock.Object);
    }

    [Fact]
    public async Task Notificar_EnviaParaAdminsEParaOsAtendentesQueVeem()
    {
        var chamadoId = Guid.NewGuid();
        var quemVe1 = Guid.NewGuid();
        var quemVe2 = Guid.NewGuid();
        _repoMock.Setup(r => r.ListarAtendentesQuePodemVerAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([quemVe1, quemVe2]);

        await _notificador.NotificarAsync(chamadoId, 42, "SlaAtrasado", "CAM-42 — PRAZO ESTOURADO!", CancellationToken.None);

        _grupos.Should().Equal(ChamadosHub.GrupoAdmins);
        _usuarios.Should().ContainSingle().Which.Should().BeEquivalentTo([quemVe1.ToString(), quemVe2.ToString()]);
        _eventos.Should().Equal("SlaAtrasado", "SlaAtrasado");
    }

    [Fact]
    public async Task Notificar_NuncaUsaOGrupoDeAtendimentoInteiro()
    {
        // AC-02: voltar ao envio para o grupo Atendimento (todo Atendente) quebra este teste.
        _repoMock.Setup(r => r.ListarAtendentesQuePodemVerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Guid.NewGuid()]);

        await _notificador.NotificarAsync(Guid.NewGuid(), 1, "SlaAtencao", "msg", CancellationToken.None);

        _grupos.Should().NotContain(ChamadosHub.GrupoAtendimento).And.NotContain("Todos");
    }

    [Fact]
    public async Task Notificar_SemAtendentesQueVeem_SoAdmins()
    {
        _repoMock.Setup(r => r.ListarAtendentesQuePodemVerAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _notificador.NotificarAsync(Guid.NewGuid(), 1, "SlaAtencao", "msg", CancellationToken.None);

        _grupos.Should().Equal(ChamadosHub.GrupoAdmins);
        _usuarios.Should().BeEmpty();
    }
}
