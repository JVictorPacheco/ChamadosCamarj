using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using FluentAssertions;

namespace ChamadosCamarj.UnitTests.Domain;

public class UsuarioPerfilTests
{
    [Fact]
    public void Criar_DeveComecarAtivo()
    {
        var usuario = new UsuarioPerfil("victor@camarj.com.br", "Victor", Perfil.Admin);
        usuario.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Criar_DeveNormalizarEmailParaMinusculoETrim()
    {
        var usuario = new UsuarioPerfil("  Victor@Camarj.COM.BR  ", "Victor", Perfil.Admin);
        usuario.Email.Should().Be("victor@camarj.com.br");
    }

    [Fact]
    public void Criar_ComEmailVazio_DeveLancarArgumentException()
    {
        var act = () => new UsuarioPerfil("", "Victor", Perfil.Admin);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Criar_ComNomeVazio_DeveLancarArgumentException()
    {
        var act = () => new UsuarioPerfil("victor@camarj.com.br", "", Perfil.Admin);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Desativar_DeveMudarAtivoParaFalse()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);

        usuario.Desativar();

        usuario.Ativo.Should().BeFalse();
    }

    [Fact]
    public void Ativar_DeveReativarUsuario()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.Desativar();

        usuario.Ativar();

        usuario.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Atualizar_DeveMudarNomeEPerfil()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);

        usuario.Atualizar("Fábio Silva", Perfil.Admin);

        usuario.Nome.Should().Be("Fábio Silva");
        usuario.Perfil.Should().Be(Perfil.Admin);
    }

    [Fact]
    public void Atualizar_ComNomeVazio_DeveLancarArgumentException()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);

        var act = () => usuario.Atualizar("", Perfil.Admin);

        act.Should().Throw<ArgumentException>();
    }

    // spec controle-de-acesso AC-16: usuário novo (ou existente) começa sem ajuste = padrão do perfil.
    [Fact]
    public void Criar_ComecaSemAjusteDeModulos()
    {
        var usuario = new UsuarioPerfil("ana@camarj.com.br", "Ana", Perfil.Atendente);

        usuario.ModulosConcedidos.Should().Be(ModuloSistema.Nenhum);
        usuario.ModulosRetirados.Should().Be(ModuloSistema.Nenhum);
        usuario.TemAjusteDeModulos.Should().BeFalse();
    }

    [Fact]
    public void AjustarModulos_EVoltarAoPadrao()
    {
        // AC-03 / AC-04
        var usuario = new UsuarioPerfil("ana@camarj.com.br", "Ana", Perfil.Atendente);

        usuario.AjustarModulos(ModuloSistema.Nenhum, ModuloSistema.RelatorioMensal);
        usuario.TemAjusteDeModulos.Should().BeTrue();
        usuario.ModulosRetirados.Should().Be(ModuloSistema.RelatorioMensal);

        usuario.VoltarAoPadraoDeModulos();
        usuario.TemAjusteDeModulos.Should().BeFalse();
    }
}
