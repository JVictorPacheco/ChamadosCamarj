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
        var aviso = _avisos.Should().ContainSingle().Subject;
        aviso.Modulos.Should().Equal("Arquivo", "Kanban", "Fila", "Dashboard");
        aviso.Perfil.Should().Be(Perfil.Atendente); // review-2 R-03: a tela compara com o perfil da sessão
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
    public async Task Salvar_MudarChat_UsaOComandoDeChatExistente_EAvisa()
    {
        // AC-09: o mesmo comando de antes (avisos aos participantes e auditoria do chat continuam).
        // A linha "Chat" da auditoria de acessos é gravada pelo próprio DefinirChatPerfilCommand (review R-06).
        var atendente = Usuario(Perfil.Atendente);

        await Executar(atendente.Id, ["Arquivo", "Kanban", "Fila", "Dashboard", "RelatorioMensal"], ChatPerfil.Participante);

        _mediatorMock.Verify(m => m.Send(
            It.Is<DefinirChatPerfilCommand>(c => c.UsuarioId == atendente.Id && c.ChatPerfil == ChatPerfil.Participante && c.AdminId == _adminId),
            It.IsAny<CancellationToken>()), Times.Once);
        _auditoria.Should().BeEmpty();
        _avisos.Should().ContainSingle().Which.ChatPerfil.Should().Be(ChatPerfil.Participante);
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

    // review R-02 (decisão do usuário): o Chat de um Admin — inclusive o do próprio — continua ajustável.
    [Fact]
    public async Task DefinirChatDeAcesso_ParaAdmin_UsaOComandoDeChat()
    {
        var admin = Usuario(Perfil.Admin);
        var handler = new DefinirChatDeAcessoCommandHandler(_usuariosMock.Object, _mediatorMock.Object);

        await handler.Handle(new DefinirChatDeAcessoCommand(admin.Id, ChatPerfil.CriadorDeGrupo, "Admin", admin.Id, "Admin"), CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(
            It.Is<DefinirChatPerfilCommand>(c => c.UsuarioId == admin.Id && c.ChatPerfil == ChatPerfil.CriadorDeGrupo),
            It.IsAny<CancellationToken>()), Times.Once);
        _avisos.Should().ContainSingle().Which.Perfil.Should().Be(Perfil.Admin);
    }

    // AC-05 em todas as rotas do Controle de acesso, não só em Salvar/Chat (review-3 R-03).
    [Theory]
    [InlineData("Atendente")]
    [InlineData("Solicitante")]
    public async Task ListarDetalharEVoltarAoPadrao_QuemNaoEhAdmin_Forbidden(string perfil)
    {
        var atendente = Usuario(Perfil.Atendente);
        atendente.AjustarModulos(ModuloSistema.Nenhum, ModuloSistema.RelatorioMensal);

        var listar = () => new ListarAcessosQueryHandler(_usuariosMock.Object)
            .Handle(new ListarAcessosQuery(perfil), CancellationToken.None);
        var detalhar = () => new ObterAcessoUsuarioQueryHandler(_usuariosMock.Object, _auditoriaMock.Object)
            .Handle(new ObterAcessoUsuarioQuery(atendente.Id, perfil), CancellationToken.None);
        var voltar = () => new VoltarAoPadraoCommandHandler(_usuariosMock.Object, _auditoriaMock.Object, _mediatorMock.Object)
            .Handle(new VoltarAoPadraoCommand(atendente.Id, perfil), CancellationToken.None);

        await listar.Should().ThrowAsync<ForbiddenException>();
        await detalhar.Should().ThrowAsync<ForbiddenException>();
        await voltar.Should().ThrowAsync<ForbiddenException>();
        atendente.TemAjusteDeModulos.Should().BeTrue();
        _usuariosMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioPerfil>(), It.IsAny<CancellationToken>()), Times.Never);
        _usuariosMock.Verify(r => r.ListarAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DefinirChatDeAcesso_QuemNaoEhAdmin_Forbidden()
    {
        var atendente = Usuario(Perfil.Atendente);
        var handler = new DefinirChatDeAcessoCommandHandler(_usuariosMock.Object, _mediatorMock.Object);

        var act = () => handler.Handle(new DefinirChatDeAcessoCommand(atendente.Id, ChatPerfil.Participante, "Atendente"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    // review R-05: pedido inválido é recusado antes de gravar.
    [Fact]
    public void Validador_RecusaChatInvalidoEModulosNulos()
    {
        var validador = new SalvarAcessosCommandValidator();
        validador.Validate(new SalvarAcessosCommand(Guid.NewGuid(), null!, (ChatPerfil)99)).Errors
            .Select(e => e.PropertyName).Should().Contain(["Modulos", "ChatPerfil"]);
    }

    // review-2 R-05: o painel estava aberto com o perfil antigo — salvar é recusado, nada muda.
    [Fact]
    public async Task Salvar_ComPerfilEsperadoDiferente_RecusaSemGravar()
    {
        var atendente = Usuario(Perfil.Atendente); // promovido depois que o painel abriu como Solicitante

        var act = () => Salvar().Handle(new SalvarAcessosCommand(atendente.Id, ["Arquivo"], ChatPerfil.SemAcesso, "Admin", _adminId, "Admin", Perfil.Solicitante), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        _usuariosMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioPerfil>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
