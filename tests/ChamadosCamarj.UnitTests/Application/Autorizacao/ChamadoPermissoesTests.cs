using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;
using FluentAssertions;

namespace ChamadosCamarj.UnitTests.Application.Autorizacao;

/// <summary>
/// Matriz de ações × perfis — spec autorizacao-chamados (AC-05, AC-06, AC-07, AC-09, AC-10, AC-11, AC-20).
/// </summary>
public class ChamadoPermissoesTests
{
    private const string EmailDono = "dono@camarj.com.br";
    private static readonly Guid Grupo = Guid.NewGuid();

    private static ContextoAcesso Ctx(Perfil perfil, string email = "outro@camarj.com.br", Guid? grupo = null) =>
        new(Guid.NewGuid(), email, perfil, grupo);

    [Theory]
    [InlineData(AcaoChamado.Ver)]
    [InlineData(AcaoChamado.ComentarPublico)]
    [InlineData(AcaoChamado.Anexar)]
    public void Solicitante_DoGrupo_PodeVerComentarEAnexarChamadoQueNaoAbriu(AcaoChamado acao)
    {
        ChamadoPermissoes.Pode(acao, Ctx(Perfil.Solicitante, grupo: Grupo), EmailDono).Should().BeTrue();
    }

    [Theory]
    [InlineData(AcaoChamado.ComentarInterno)]
    [InlineData(AcaoChamado.Assumir)]
    [InlineData(AcaoChamado.Resolver)]
    [InlineData(AcaoChamado.Encerrar)]
    [InlineData(AcaoChamado.Reabrir)]
    [InlineData(AcaoChamado.AlterarStatus)]
    [InlineData(AcaoChamado.Editar)]
    [InlineData(AcaoChamado.Reatribuir)]
    [InlineData(AcaoChamado.AlterarPrioridade)]
    [InlineData(AcaoChamado.ForcarEncerramento)]
    public void Solicitante_NaoPodeAcoesDeAtendimento_NemNoProprioChamado(AcaoChamado acao)
    {
        ChamadoPermissoes.Pode(acao, Ctx(Perfil.Solicitante, email: EmailDono), EmailDono).Should().BeFalse();
    }

    [Fact]
    public void Solicitante_PodeCancelarChamadoQueAbriu_IgnorandoMaiusculas()
    {
        ChamadoPermissoes.Pode(AcaoChamado.Cancelar, Ctx(Perfil.Solicitante, email: "DONO@camarj.com.br"), EmailDono)
            .Should().BeTrue();
    }

    [Fact]
    public void Solicitante_DoGrupo_NaoPodeCancelarChamadoDoColega()
    {
        ChamadoPermissoes.Pode(AcaoChamado.Cancelar, Ctx(Perfil.Solicitante, grupo: Grupo), EmailDono)
            .Should().BeFalse();
    }

    [Fact]
    public void Solicitante_SemEmailNoToken_NaoPodeCancelar()
    {
        ChamadoPermissoes.Pode(AcaoChamado.Cancelar, Ctx(Perfil.Solicitante, email: ""), "").Should().BeFalse();
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
    [InlineData(AcaoChamado.Editar)]
    public void Atendente_PodeAcoesDeAtendimento(AcaoChamado acao)
    {
        ChamadoPermissoes.Pode(acao, Ctx(Perfil.Atendente), EmailDono).Should().BeTrue();
    }

    [Theory]
    [InlineData(AcaoChamado.Reatribuir)]
    [InlineData(AcaoChamado.AlterarPrioridade)]
    [InlineData(AcaoChamado.ForcarEncerramento)]
    public void Atendente_NaoPodeAcoesExclusivasDoAdmin(AcaoChamado acao)
    {
        ChamadoPermissoes.Pode(acao, Ctx(Perfil.Atendente, grupo: Grupo), EmailDono).Should().BeFalse();
    }

    [Fact]
    public void Admin_ComGrupo_PodeTodasAsAcoes()
    {
        var admin = Ctx(Perfil.Admin, grupo: Grupo);

        foreach (var acao in Enum.GetValues<AcaoChamado>())
            ChamadoPermissoes.Pode(acao, admin, EmailDono).Should().BeTrue($"Admin deve poder {acao}");
    }

    [Fact]
    public void Solicitante_PodeVer_EhCoberturaCompletaDoEnum()
    {
        // Garante que uma ação nova no enum não fica liberada por engano para Solicitante:
        // só estas 4 podem ser true (Cancelar depende de ser o dono).
        var permitidas = Enum.GetValues<AcaoChamado>()
            .Where(a => ChamadoPermissoes.Pode(a, Ctx(Perfil.Solicitante, email: EmailDono), EmailDono));

        permitidas.Should().BeEquivalentTo(new[]
        {
            AcaoChamado.Ver, AcaoChamado.ComentarPublico, AcaoChamado.Anexar, AcaoChamado.Cancelar
        });
    }
}
