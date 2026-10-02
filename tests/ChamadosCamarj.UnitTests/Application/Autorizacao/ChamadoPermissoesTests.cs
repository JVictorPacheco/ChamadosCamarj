using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;
using FluentAssertions;

namespace ChamadosCamarj.UnitTests.Application.Autorizacao;

/// <summary>
/// Matriz de ações × perfis — spec autorizacao-chamados (AC-05, AC-06, AC-07, AC-09, AC-10, AC-11) e, para
/// Editar, spec editar-chamado (substitui o AC-20 de autorizacao-chamados).
/// </summary>
public class ChamadoPermissoesTests
{
    private const string EmailDono = "dono@camarj.com.br";
    private static readonly Guid Grupo = Guid.NewGuid();

    // Chamado aberto pelo "dono", sem responsável, em aberto: a situação padrão das regras antigas.
    private static readonly DadosDoChamado Dono = new(EmailDono, null, StatusChamado.Aberto);

    private static ContextoAcesso Ctx(Perfil perfil, string email = "outro@camarj.com.br", Guid? grupo = null) =>
        new(Guid.NewGuid(), email, perfil, grupo);

    [Theory]
    [InlineData(AcaoChamado.Ver)]
    [InlineData(AcaoChamado.ComentarPublico)]
    [InlineData(AcaoChamado.Anexar)]
    public void Solicitante_DoGrupo_PodeVerComentarEAnexarChamadoQueNaoAbriu(AcaoChamado acao)
    {
        ChamadoPermissoes.Pode(acao, Ctx(Perfil.Solicitante, grupo: Grupo), Dono).Should().BeTrue();
    }

    [Theory]
    [InlineData(AcaoChamado.ComentarInterno)]
    [InlineData(AcaoChamado.Assumir)]
    [InlineData(AcaoChamado.Resolver)]
    [InlineData(AcaoChamado.Encerrar)]
    [InlineData(AcaoChamado.Reabrir)]
    [InlineData(AcaoChamado.AlterarStatus)]
    [InlineData(AcaoChamado.ReclassificarTipo)]
    [InlineData(AcaoChamado.Reatribuir)]
    [InlineData(AcaoChamado.AlterarPrioridade)]
    [InlineData(AcaoChamado.ForcarEncerramento)]
    public void Solicitante_NaoPodeAcoesDeAtendimento_NemNoProprioChamado(AcaoChamado acao)
    {
        ChamadoPermissoes.Pode(acao, Ctx(Perfil.Solicitante, email: EmailDono), Dono).Should().BeFalse();
    }

    [Fact]
    public void Solicitante_PodeCancelarChamadoQueAbriu_IgnorandoMaiusculas()
    {
        ChamadoPermissoes.Pode(AcaoChamado.Cancelar, Ctx(Perfil.Solicitante, email: "DONO@camarj.com.br"), Dono)
            .Should().BeTrue();
    }

    [Fact]
    public void Solicitante_DoGrupo_NaoPodeCancelarChamadoDoColega()
    {
        ChamadoPermissoes.Pode(AcaoChamado.Cancelar, Ctx(Perfil.Solicitante, grupo: Grupo), Dono)
            .Should().BeFalse();
    }

    [Fact]
    public void Solicitante_SemEmailNoToken_NaoPodeCancelar()
    {
        ChamadoPermissoes.Pode(AcaoChamado.Cancelar, Ctx(Perfil.Solicitante, email: ""), new DadosDoChamado("", null, StatusChamado.Aberto)).Should().BeFalse();
    }

    [Theory]
    [InlineData(AcaoChamado.Ver)]
    [InlineData(AcaoChamado.ComentarPublico)]
    [InlineData(AcaoChamado.ComentarInterno)]
    [InlineData(AcaoChamado.Anexar)]
    [InlineData(AcaoChamado.Assumir)]
    [InlineData(AcaoChamado.Resolver)]
    [InlineData(AcaoChamado.Encerrar)]
    [InlineData(AcaoChamado.Reabrir)]
    [InlineData(AcaoChamado.AlterarStatus)]
    [InlineData(AcaoChamado.Cancelar)]
    [InlineData(AcaoChamado.ReclassificarTipo)]
    public void Atendente_PodeAcoesDeAtendimento(AcaoChamado acao)
    {
        ChamadoPermissoes.Pode(acao, Ctx(Perfil.Atendente), Dono).Should().BeTrue();
    }

    [Theory]
    [InlineData(AcaoChamado.Reatribuir)]
    [InlineData(AcaoChamado.AlterarPrioridade)]
    [InlineData(AcaoChamado.ForcarEncerramento)]
    public void Atendente_NaoPodeAcoesExclusivasDoAdmin(AcaoChamado acao)
    {
        ChamadoPermissoes.Pode(acao, Ctx(Perfil.Atendente, grupo: Grupo), Dono).Should().BeFalse();
    }

    [Fact]
    public void Admin_ComGrupo_PodeTodasAsAcoes()
    {
        var admin = Ctx(Perfil.Admin, grupo: Grupo);

        foreach (var acao in Enum.GetValues<AcaoChamado>())
            ChamadoPermissoes.Pode(acao, admin, Dono).Should().BeTrue($"Admin deve poder {acao}");
    }

    [Fact]
    public void Solicitante_PodeVer_EhCoberturaCompletaDoEnum()
    {
        // Garante que uma ação nova no enum não fica liberada por engano para Solicitante:
        // só estas 5 podem ser true (Cancelar e Editar dependem de ser o dono; Editar, também de ninguém ter
        // assumido — spec editar-chamado).
        var permitidas = Enum.GetValues<AcaoChamado>()
            .Where(a => ChamadoPermissoes.Pode(a, Ctx(Perfil.Solicitante, email: EmailDono), Dono));

        permitidas.Should().BeEquivalentTo(new[]
        {
            AcaoChamado.Ver, AcaoChamado.ComentarPublico, AcaoChamado.Anexar, AcaoChamado.Cancelar, AcaoChamado.Editar
        });
    }

    // ── Editar (spec editar-chamado) ─────────────────────────────────────────

    private static readonly Guid IdAtendenteA = Guid.NewGuid();

    private static ContextoAcesso Pessoa(Perfil perfil, string email, Guid? id = null, Guid? grupo = null) =>
        new(id ?? Guid.NewGuid(), email, perfil, grupo);

    private static DadosDoChamado Chamado(Guid? responsavel = null, StatusChamado status = StatusChamado.Aberto) =>
        new(EmailDono, responsavel, status);

    [Theory]
    [InlineData(Perfil.Solicitante)]
    [InlineData(Perfil.Atendente)]
    public void Editar_QuemAbriu_PodeEnquantoNinguemAssumiu(Perfil perfil)
    {
        // AC-01 e AC-03: vale para qualquer perfil que abriu o chamado.
        ChamadoPermissoes.Pode(AcaoChamado.Editar, Pessoa(perfil, "DONO@camarj.com.br"), Chamado()).Should().BeTrue();
    }

    [Fact]
    public void Editar_QuemAbriu_NaoPodeDepoisQueAlguemAssumiu()
    {
        // AC-02
        ChamadoPermissoes.Pode(AcaoChamado.Editar, Pessoa(Perfil.Solicitante, EmailDono),
            Chamado(IdAtendenteA, StatusChamado.EmAndamento)).Should().BeFalse();
    }

    [Fact]
    public void Editar_SolicitanteColegaDeGrupo_NaoPode()
    {
        // AC-04: vê o chamado do colega, mas só quem abriu edita.
        ChamadoPermissoes.Pode(AcaoChamado.Editar, Pessoa(Perfil.Solicitante, "colega@camarj.com.br", grupo: Grupo),
            Chamado()).Should().BeFalse();
    }

    [Fact]
    public void Editar_ResponsavelAtual_Pode()
    {
        // AC-05 e AC-07 (inclusive quem abriu e assumiu o próprio chamado).
        ChamadoPermissoes.Pode(AcaoChamado.Editar, Pessoa(Perfil.Atendente, "a@camarj.com.br", IdAtendenteA),
            Chamado(IdAtendenteA, StatusChamado.EmAndamento)).Should().BeTrue();
        ChamadoPermissoes.Pode(AcaoChamado.Editar, Pessoa(Perfil.Atendente, EmailDono, IdAtendenteA),
            Chamado(IdAtendenteA, StatusChamado.EmAndamento)).Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Editar_OutroAtendenteQueVe_NaoPode(bool chamadoTemResponsavel)
    {
        // AC-06 e AC-08: só o responsável atual; sem responsável, só quem abriu.
        var outro = Pessoa(Perfil.Atendente, "b@camarj.com.br", grupo: Grupo);
        var chamado = chamadoTemResponsavel ? Chamado(IdAtendenteA, StatusChamado.EmAndamento) : Chamado();

        ChamadoPermissoes.Pode(AcaoChamado.Editar, outro, chamado).Should().BeFalse();
    }

    [Theory]
    [InlineData(StatusChamado.Aberto, false)]
    [InlineData(StatusChamado.EmAndamento, true)]
    public void Editar_Admin_PodeQualquerChamadoNaoEncerrado(StatusChamado status, bool comResponsavel)
    {
        // AC-09
        var admin = Pessoa(Perfil.Admin, "admin@camarj.com.br", grupo: Grupo);
        ChamadoPermissoes.Pode(AcaoChamado.Editar, admin, Chamado(comResponsavel ? IdAtendenteA : null, status))
            .Should().BeTrue();
    }

    [Theory]
    [InlineData(StatusChamado.Resolvido)]
    [InlineData(StatusChamado.Fechado)]
    [InlineData(StatusChamado.Cancelado)]
    public void Editar_Encerrado_NinguemPode_NemAdminNemResponsavel(StatusChamado status)
    {
        // AC-10
        var chamado = Chamado(IdAtendenteA, status);
        ChamadoPermissoes.EdicaoBloqueadaPorEncerramento(chamado).Should().BeTrue();
        ChamadoPermissoes.Pode(AcaoChamado.Editar, Pessoa(Perfil.Admin, "admin@camarj.com.br"), chamado).Should().BeFalse();
        ChamadoPermissoes.Pode(AcaoChamado.Editar, Pessoa(Perfil.Atendente, "a@camarj.com.br", IdAtendenteA), chamado).Should().BeFalse();
        ChamadoPermissoes.Pode(AcaoChamado.Editar, Pessoa(Perfil.Solicitante, EmailDono), new DadosDoChamado(EmailDono, null, status)).Should().BeFalse();
    }

    [Fact]
    public void Editar_Reaberto_QuemAbriuVoltaAPoder()
    {
        // AC-11: reabrir deixa Em andamento e sem responsável.
        ChamadoPermissoes.Pode(AcaoChamado.Editar, Pessoa(Perfil.Solicitante, EmailDono),
            Chamado(null, StatusChamado.EmAndamento)).Should().BeTrue();
    }

    [Theory]
    [InlineData(AcaoChamado.Editar, Perfil.Admin, true)]
    [InlineData(AcaoChamado.Editar, Perfil.Atendente, true)]
    [InlineData(AcaoChamado.Cancelar, Perfil.Solicitante, true)]
    [InlineData(AcaoChamado.Cancelar, Perfil.Atendente, false)]
    [InlineData(AcaoChamado.Resolver, Perfil.Atendente, false)]
    [InlineData(AcaoChamado.Ver, Perfil.Solicitante, false)]
    public void DependeDoChamado_SoEditarECancelarDoSolicitante(AcaoChamado acao, Perfil perfil, bool esperado)
    {
        ChamadoPermissoes.DependeDoChamado(acao, perfil).Should().Be(esperado);
    }
}
