using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Domain.Enums;
using FluentAssertions;
using static ChamadosCamarj.Domain.Enums.ModuloSistema;

namespace ChamadosCamarj.UnitTests.Application.Acessos;

/// <summary>Regra de módulos — spec controle-de-acesso AC-02..AC-08, AC-16.</summary>
public class ModulosDeAcessoTests
{
    [Fact]
    public void Padroes_SaoIguaisAoMenuDeAntesDaFeature()
    {
        // AC-16: sem ajuste, ninguém ganha nem perde nada no deploy. Matriz de antes:
        // Solicitante = Arquivo; Atendente e Admin = Arquivo, Kanban, Fila, Dashboard, Relatório.
        ModulosDeAcesso.Efetivos(Perfil.Solicitante, Nenhum, Nenhum).Should().Be(Arquivo);
        ModulosDeAcesso.Efetivos(Perfil.Atendente, Nenhum, Nenhum).Should().Be(Arquivo | Kanban | Fila | Dashboard | RelatorioMensal);
        ModulosDeAcesso.Efetivos(Perfil.Admin, Nenhum, Nenhum).Should().Be(Arquivo | Kanban | Fila | Dashboard | RelatorioMensal);
    }

    [Fact]
    public void Admin_SempreTemTudo_MesmoComAjusteGravado()
    {
        // AC-06
        ModulosDeAcesso.Efetivos(Perfil.Admin, Nenhum, Todos).Should().Be(Todos);
    }

    [Fact]
    public void Atendente_PodePerderUmModulo()
    {
        // AC-03
        ModulosDeAcesso.Efetivos(Perfil.Atendente, Nenhum, RelatorioMensal)
            .Should().Be(Arquivo | Kanban | Fila | Dashboard);
    }

    [Fact]
    public void Solicitante_PodeGanharModulosDeConsulta()
    {
        // AC-07
        ModulosDeAcesso.Efetivos(Perfil.Solicitante, Dashboard | RelatorioMensal, Nenhum)
            .Should().Be(Arquivo | Dashboard | RelatorioMensal);
    }

    [Fact]
    public void Solicitante_NuncaTemKanbanNemFila_MesmoComAjusteGravadoErrado()
    {
        // AC-08: o efetivo é sempre limitado ao permitido para o perfil.
        ModulosDeAcesso.Efetivos(Perfil.Solicitante, Kanban | Fila, Nenhum).Should().Be(Arquivo);
    }

    [Fact]
    public void CalcularAjustes_GuardaSoAsExcecoesAoPadrao()
    {
        ModulosDeAcesso.CalcularAjustes(Perfil.Atendente, Arquivo | Kanban | Fila | Dashboard)
            .Should().Be((Nenhum, RelatorioMensal));
        ModulosDeAcesso.CalcularAjustes(Perfil.Solicitante, Arquivo | Dashboard)
            .Should().Be((Dashboard, Nenhum));
        ModulosDeAcesso.CalcularAjustes(Perfil.Solicitante, Dashboard)
            .Should().Be((Dashboard, Arquivo));
        ModulosDeAcesso.CalcularAjustes(Perfil.Atendente, Todos).Should().Be((Nenhum, Nenhum));
    }

    [Fact]
    public void CalcularAjustes_RecusaModuloNaoAjustavelParaOPerfil()
    {
        var act = () => ModulosDeAcesso.CalcularAjustes(Perfil.Solicitante, Arquivo | Kanban);
        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void CalcularAjustes_RecusaAdmin()
    {
        var act = () => ModulosDeAcesso.CalcularAjustes(Perfil.Admin, Arquivo);
        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void Nomes_IdaEVolta()
    {
        var nomes = ModulosDeAcesso.Nomes(Arquivo | Dashboard);
        nomes.Should().Equal("Arquivo", "Dashboard");
        ModulosDeAcesso.DeNomes(nomes).Should().Be(Arquivo | Dashboard);
        var act = () => ModulosDeAcesso.DeNomes(["Todos"]);
        act.Should().Throw<BadRequestException>();
    }
}
