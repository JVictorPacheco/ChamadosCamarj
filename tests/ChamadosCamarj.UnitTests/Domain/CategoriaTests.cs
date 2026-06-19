using ChamadosCamarj.Domain.Entities;
using FluentAssertions;

namespace ChamadosCamarj.UnitTests.Domain;

public class CategoriaTests
{
    [Fact]
    public void Criar_DeveComecarAtiva()
    {
        var categoria = new Categoria("Autorização", "Pedidos de autorização");
        categoria.Ativa.Should().BeTrue();
    }

    [Fact]
    public void Criar_ComNomeVazio_DeveLancarArgumentException()
    {
        var act = () => new Categoria("", "Descrição");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Desativar_DeveMudarAtivaParaFalse()
    {
        var categoria = new Categoria("Financeiro", "Assuntos financeiros");

        categoria.Desativar();

        categoria.Ativa.Should().BeFalse();
    }

    [Fact]
    public void Ativar_DeveReativarCategoria()
    {
        var categoria = new Categoria("Financeiro", "Assuntos financeiros");
        categoria.Desativar();

        categoria.Ativar();

        categoria.Ativa.Should().BeTrue();
    }

    [Fact]
    public void Atualizar_DeveMudarNomeEDescricao()
    {
        var categoria = new Categoria("Nome antigo", "Desc antiga");

        categoria.Atualizar("Nome novo", "Desc nova");

        categoria.Nome.Should().Be("Nome novo");
        categoria.Descricao.Should().Be("Desc nova");
    }

    [Fact]
    public void Atualizar_ComNomeVazio_DeveLancarArgumentException()
    {
        var categoria = new Categoria("Financeiro", "Assuntos financeiros");

        var act = () => categoria.Atualizar("", "Nova desc");

        act.Should().Throw<ArgumentException>();
    }
}
