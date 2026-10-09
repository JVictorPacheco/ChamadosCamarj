using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.WebApi.Hubs;
using ChamadosCamarj.WebApi.Notifications;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace ChamadosCamarj.UnitTests.WebApi.Notifications;

/// <summary>spec perfil-no-cadastro AC-13..15: o tempo real acompanha o cadastro de quem já está conectado.</summary>
public class CadastroDeAcessoAlteradoNotificationHandlerTests
{
    private readonly Guid _usuario = Guid.NewGuid();
    private readonly ConexoesTempoReal _conexoes = new();
    private readonly Mock<IGroupManager> _grupos = new();
    private readonly CadastroDeAcessoAlteradoNotificationHandler _handler;
    private readonly List<(string conexao, string grupo)> _entrou = [];
    private readonly List<(string conexao, string grupo)> _saiu = [];

    public CadastroDeAcessoAlteradoNotificationHandlerTests()
    {
        _grupos.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((c, g, _) => _entrou.Add((c, g))).Returns(Task.CompletedTask);
        _grupos.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((c, g, _) => _saiu.Add((c, g))).Returns(Task.CompletedTask);
        var hub = new Mock<IHubContext<ChamadosHub>>();
        hub.Setup(h => h.Groups).Returns(_grupos.Object);
        _handler = new CadastroDeAcessoAlteradoNotificationHandler(hub.Object, _conexoes);
    }

    private Mock<HubCallerContext> Conectar(string connectionId, string hubNome, Guid? usuario = null)
    {
        var contexto = new Mock<HubCallerContext>();
        contexto.SetupGet(c => c.ConnectionId).Returns(connectionId);
        contexto.SetupGet(c => c.UserIdentifier).Returns((usuario ?? _usuario).ToString());
        _conexoes.Registrar(contexto.Object, hubNome);
        return contexto;
    }

    private Task Avisar(Perfil perfil, bool ativo = true) =>
        _handler.Handle(new CadastroDeAcessoAlteradoNotification(_usuario, perfil, ativo, null), CancellationToken.None);

    [Fact]
    public async Task AdminRebaixadoParaSolicitante_SaiDeAdminsEDoAtendimento()
    {
        Conectar("a", nameof(ChamadosHub));

        await Avisar(Perfil.Solicitante);

        _saiu.Should().Contain([("a", ChamadosHub.GrupoAdmins), ("a", ChamadosHub.GrupoAtendimento)]);
        _entrou.Should().BeEmpty();
    }

    [Fact]
    public async Task SolicitantePromovidoParaAtendente_EntraNoAtendimentoENaoEmAdmins()
    {
        Conectar("a", nameof(ChamadosHub));

        await Avisar(Perfil.Atendente);

        _entrou.Should().ContainSingle().Which.Should().Be(("a", ChamadosHub.GrupoAtendimento));
        _saiu.Should().ContainSingle().Which.Should().Be(("a", ChamadosHub.GrupoAdmins));
    }

    [Fact]
    public async Task AdminRebaixadoParaAtendente_PerdeSoOsAlertasDeAdmins()
    {
        Conectar("a", nameof(ChamadosHub));

        await Avisar(Perfil.Atendente);

        _saiu.Should().ContainSingle().Which.Should().Be(("a", ChamadosHub.GrupoAdmins));
    }

    [Fact]
    public async Task ContaDesativada_DerrubaAsConexoesDosDoisHubs()
    {
        var chamados = Conectar("a", nameof(ChamadosHub));
        var chat = Conectar("b", nameof(ChatHub));
        var outraPessoa = Conectar("c", nameof(ChamadosHub), Guid.NewGuid());

        await Avisar(Perfil.Atendente, ativo: false);

        chamados.Verify(c => c.Abort(), Times.Once);
        chat.Verify(c => c.Abort(), Times.Once);
        outraPessoa.Verify(c => c.Abort(), Times.Never);
    }

    [Fact]
    public async Task ContaReativada_NaoDerrubaNada()
    {
        var conexao = Conectar("a", nameof(ChamadosHub));

        await Avisar(Perfil.Atendente, ativo: true);

        conexao.Verify(c => c.Abort(), Times.Never);
    }

    [Fact]
    public async Task ConexaoDoChat_NaoTemGrupoQueDependaDePerfil()
    {
        Conectar("b", nameof(ChatHub));

        await Avisar(Perfil.Solicitante);

        _entrou.Should().BeEmpty();
        _saiu.Should().BeEmpty();
    }

    [Fact]
    public async Task OutraPessoaConectada_NaoEhAfetada()
    {
        Conectar("z", nameof(ChamadosHub), Guid.NewGuid());

        await Avisar(Perfil.Solicitante);

        _entrou.Should().BeEmpty();
        _saiu.Should().BeEmpty();
    }
}
