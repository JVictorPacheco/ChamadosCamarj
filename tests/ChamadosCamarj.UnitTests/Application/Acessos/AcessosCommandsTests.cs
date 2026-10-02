using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Acessos.Commands;
using ChamadosCamarj.Application.Features.Acessos.Queries;
using ChamadosCamarj.Application.Features.Chat.Commands.DefinirChatPerfil;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Acessos;

/// <summary>API do Controle de acesso — spec controle-de-acesso AC-01..AC-09, AC-14.</summary>
public class AcessosCommandsTests
{
    private readonly Mock<IUsuarioPerfilRepository> _usuariosMock = new();
    private readonly Mock<IAuditoriaAcessoRepository> _auditoriaMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly List<AuditoriaAcesso> _auditoria = [];
    private readonly List<AcessosAtualizadosNotification> _avisos = [];
    private readonly Guid _adminId = Guid.NewGuid();

    public AcessosCommandsTests()
    {
        _auditoriaMock.Setup(a => a.AdicionarAsync(It.IsAny<IEnumerable<AuditoriaAcesso>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<AuditoriaAcesso>, CancellationToken>((r, _) => _auditoria.AddRange(r))
            .Returns(Task.CompletedTask);
        _mediatorMock.Setup(m => m.Publish(It.IsAny<AcessosAtualizadosNotification>(), It.IsAny<CancellationToken>()))
            .Callback<AcessosAtualizadosNotification, CancellationToken>((n, _) => _avisos.Add(n))
            .Returns(Task.CompletedTask);
    }

    private UsuarioPerfil Usuario(Perfil perfil)
    {
        var usuario = new UsuarioPerfil($"{perfil}@camarj.com.br".ToLowerInvariant(), $"Pessoa {perfil}", perfil);
        _usuariosMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);
        return usuario;
    }

    private SalvarAcessosCommandHandler Salvar() => new(_usuariosMock.Object, _auditoriaMock.Object, _mediatorMock.Object);

    private Task Executar(Guid usuarioId, string[] modulos, ChatPerfil chat = ChatPerfil.SemAcesso, string perfil = "Admin") =>
        Salvar().Handle(new SalvarAcessosCommand(usuarioId, modulos, chat, perfil, _adminId, "Admin"), CancellationToken.None);

    [Fact]
    public async Task Salvar_TirarModuloDeAtendente_GravaAjusteAuditaEAvisa()
    {
        // AC-03, AC-14, AC-11
        var atendente = Usuario(Perfil.Atendente);

        await Executar(atendente.Id, ["Arquivo", "Kanban", "Fila", "Dashboard"]);

        atendente.ModulosRetirados.Should().Be(ModuloSistema.RelatorioMensal);
        _usuariosMock.Verify(r => r.AtualizarAsync(atendente, It.IsAny<CancellationToken>()), Times.Once);
        var linha = _auditoria.Should().ContainSingle().Subject;
        (linha.Item, linha.Anterior, linha.Novo).Should().Be(("Relatório mensal", "ligado", "desligado"));
        linha.UsuarioId.Should().Be(atendente.Id);
        _avisos.Should().ContainSingle().Which.Modulos.Should().Equal("Arquivo", "Kanban", "Fila", "Dashboard");
    }

    [Fact]
    public async Task Salvar_DarDashboardASolicitante()
    {
        // AC-07
        var solicitante = Usuario(Perfil.Solicitante);

        await Executar(solicitante.Id, ["Arquivo", "Dashboard"]);

        solicitante.ModulosConcedidos.Should().Be(ModuloSistema.Dashboard);
        _auditoria.Should().ContainSingle().Which.Item.Should().Be("Dashboard");
    }

    [Fact]
    public async Task Salvar_KanbanParaSolicitante_Recusa()
    {
        // AC-08
        var solicitante = Usuario(Perfil.Solicitante);

        var act = () => Executar(solicitante.Id, ["Arquivo", "Kanban"]);

        await act.Should().ThrowAsync<BadRequestException>();
        _usuariosMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioPerfil>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Salvar_AlvoAdmin_Recusa()
    {
        // AC-06
        var admin = Usuario(Perfil.Admin);
        var act = () => Executar(admin.Id, ["Arquivo"]);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Salvar_OProprioAdmin_Recusa()
    {
        // AC-06: o Admin não ajusta a si mesmo.
        var act = () => Salvar().Handle(new SalvarAcessosCommand(_adminId, ["Arquivo"], ChatPerfil.SemAcesso, "Admin", _adminId, "Admin"), CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Theory]
    [InlineData("Atendente")]
    [InlineData("Solicitante")]
    public async Task Salvar_QuemNaoEhAdmin_Forbidden(string perfil)
    {
        // AC-05
        var atendente = Usuario(Perfil.Atendente);
        var act = () => Executar(atendente.Id, ["Arquivo"], perfil: perfil);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Salvar_MudarChat_UsaOComandoDeChatExistente_EAudita()
    {
        // AC-09: o mesmo comando de antes (avisos aos participantes e auditoria do chat continuam).
        var atendente = Usuario(Perfil.Atendente);

        await Executar(atendente.Id, ["Arquivo", "Kanban", "Fila", "Dashboard", "RelatorioMensal"], ChatPerfil.Participante);

        _mediatorMock.Verify(m => m.Send(
            It.Is<DefinirChatPerfilCommand>(c => c.UsuarioId == atendente.Id && c.ChatPerfil == ChatPerfil.Participante && c.AdminId == _adminId),
            It.IsAny<CancellationToken>()), Times.Once);
        var linha = _auditoria.Should().ContainSingle().Subject;
        (linha.Item, linha.Anterior, linha.Novo).Should().Be(("Chat", "Sem acesso", "Participante"));
        _usuariosMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioPerfil>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Salvar_SemMudanca_NaoGravaNemAvisa()
    {
        var atendente = Usuario(Perfil.Atendente);

        await Executar(atendente.Id, ["Arquivo", "Kanban", "Fila", "Dashboard", "RelatorioMensal"]);

        _usuariosMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioPerfil>(), It.IsAny<CancellationToken>()), Times.Never);
        _auditoria.Should().BeEmpty();
        _avisos.Should().BeEmpty();
    }

    [Fact]
    public async Task VoltarAoPadrao_RemoveAjustesAuditaEAvisa()
    {
        // AC-04
        var atendente = Usuario(Perfil.Atendente);
        atendente.AjustarModulos(ModuloSistema.Nenhum, ModuloSistema.Kanban | ModuloSistema.Fila);
        var handler = new VoltarAoPadraoCommandHandler(_usuariosMock.Object, _auditoriaMock.Object, _mediatorMock.Object);

        await handler.Handle(new VoltarAoPadraoCommand(atendente.Id, "Admin", _adminId, "Admin"), CancellationToken.None);

        atendente.TemAjusteDeModulos.Should().BeFalse();
        _auditoria.Select(a => a.Item).Should().BeEquivalentTo(["Kanban", "Fila"]);
        _avisos.Should().ContainSingle();
    }

    [Fact]
    public async Task ObterDetalhe_Solicitante_NaoMostraKanbanNemFila()
    {
        // AC-02 / AC-08
        var solicitante = Usuario(Perfil.Solicitante);
        _auditoriaMock.Setup(a => a.ListarPorUsuarioAsync(solicitante.Id, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var handler = new ObterAcessoUsuarioQueryHandler(_usuariosMock.Object, _auditoriaMock.Object);

        var detalhe = await handler.Handle(new ObterAcessoUsuarioQuery(solicitante.Id, "Admin"), CancellationToken.None);

        detalhe.Modulos.Select(m => m.Modulo).Should().Equal("Arquivo", "Dashboard", "RelatorioMensal");
        detalhe.Modulos.Single(m => m.Modulo == "Arquivo").Should().Be(new ChamadosCamarj.Application.Features.Acessos.DTOs.ModuloAcessoItem("Arquivo", true, true, true));
        detalhe.Modulos.Single(m => m.Modulo == "Dashboard").Padrao.Should().BeFalse();
    }

    [Fact]
    public async Task Listar_AdminComoAcessoTotal()
    {
        // AC-01 / AC-06
        var admin = new UsuarioPerfil("adm@camarj.com.br", "Adm", Perfil.Admin);
        var atendente = new UsuarioPerfil("ate@camarj.com.br", "Ate", Perfil.Atendente);
        atendente.AjustarModulos(ModuloSistema.Nenhum, ModuloSistema.Fila);
        _usuariosMock.Setup(r => r.ListarAsync(It.IsAny<CancellationToken>())).ReturnsAsync([admin, atendente]);
        var handler = new ListarAcessosQueryHandler(_usuariosMock.Object);

        var lista = await handler.Handle(new ListarAcessosQuery("Admin"), CancellationToken.None);

        lista.Single(l => l.Id == admin.Id).AcessoTotal.Should().BeTrue();
        var linha = lista.Single(l => l.Id == atendente.Id);
        linha.TemAjuste.Should().BeTrue();
        linha.Modulos.Should().NotContain("Fila");
    }
}
