using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using FluentAssertions;

namespace ChamadosCamarj.UnitTests.Domain;

public class ChamadoTests
{
    private static readonly Guid CategoriaId = Guid.NewGuid();

    private static Chamado CriarChamado(PrioridadeChamado prioridade = PrioridadeChamado.Media)
        => new("Título teste", "Descrição teste", "João", "joao@camarj.com.br", CategoriaId, prioridade);

    // ── Construtor ────────────────────────────────────────────────────────────

    [Fact]
    public void Criar_DeveTerStatusAberto()
    {
        var chamado = CriarChamado();
        chamado.Status.Should().Be(StatusChamado.Aberto);
    }

    [Theory]
    [InlineData("", "Descrição")]
    [InlineData("Título", "")]
    public void Criar_ComCamposObrigatoriosVazios_DeveLancarArgumentException(string titulo, string descricao)
    {
        var act = () => new Chamado(titulo, descricao, "João", "joao@camarj.com.br", CategoriaId);
        act.Should().Throw<ArgumentException>();
    }

    // ── SLA ──────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(PrioridadeChamado.Urgente, 8)]
    [InlineData(PrioridadeChamado.Alta, 24)]
    [InlineData(PrioridadeChamado.Media, 16)]
    [InlineData(PrioridadeChamado.Baixa, 48)]
    public void Criar_DeveCalcularDataLimiteConforme_Prioridade(PrioridadeChamado prioridade, int horas)
    {
        var antes = DateTime.UtcNow;
        var chamado = CriarChamado(prioridade);
        var depois = DateTime.UtcNow;

        chamado.DataLimite.Should().NotBeNull();
        chamado.DataLimite!.Value.Should().BeOnOrAfter(antes.AddHours(horas));
        chamado.DataLimite!.Value.Should().BeOnOrBefore(depois.AddHours(horas).AddSeconds(1));
    }

    // ── Atribuir ─────────────────────────────────────────────────────────────

    [Fact]
    public void Atribuir_DeveDefinirResponsavelEMudarStatusParaEmAndamento()
    {
        var chamado = CriarChamado();
        var responsavelId = Guid.NewGuid();

        chamado.Atribuir(responsavelId, "Victor");

        chamado.Status.Should().Be(StatusChamado.EmAndamento);
        chamado.ResponsavelId.Should().Be(responsavelId);
        chamado.ResponsavelNome.Should().Be("Victor");
    }

    // ── Resolver ─────────────────────────────────────────────────────────────

    [Fact]
    public void Resolver_DeveMudarStatusParaResolvidoEPreencherDataConclusao()
    {
        var chamado = CriarChamado();
        chamado.Atribuir(Guid.NewGuid(), "Victor");

        var antes = DateTime.UtcNow;
        chamado.Resolver();

        chamado.Status.Should().Be(StatusChamado.Resolvido);
        chamado.DataConclusao.Should().NotBeNull();
        chamado.DataConclusao!.Value.Should().BeOnOrAfter(antes);
    }

    // ── Fechar ───────────────────────────────────────────────────────────────

    [Fact]
    public void Fechar_DeveMudarStatusParaFechado()
    {
        var chamado = CriarChamado();
        chamado.Atribuir(Guid.NewGuid(), "Victor");
        chamado.Resolver();

        chamado.Fechar();

        chamado.Status.Should().Be(StatusChamado.Fechado);
    }

    // ── Cancelar ─────────────────────────────────────────────────────────────

    [Fact]
    public void Cancelar_DeAberto_DeveMudarStatusParaCancelado()
    {
        var chamado = CriarChamado();

        chamado.Cancelar();

        chamado.Status.Should().Be(StatusChamado.Cancelado);
    }

    [Fact]
    public void Cancelar_DeEmAndamento_DeveMudarStatusParaCancelado()
    {
        var chamado = CriarChamado();
        chamado.Atribuir(Guid.NewGuid(), "Victor");

        chamado.Cancelar();

        chamado.Status.Should().Be(StatusChamado.Cancelado);
    }

    // ── Reabrir ──────────────────────────────────────────────────────────────

    [Fact]
    public void Reabrir_DeveVoltarParaAbertoELimparResponsavel()
    {
        var chamado = CriarChamado();
        chamado.Atribuir(Guid.NewGuid(), "Victor");
        chamado.Resolver();

        chamado.Reabrir();

        chamado.Status.Should().Be(StatusChamado.Aberto);
        chamado.ResponsavelId.Should().BeNull();
        chamado.ResponsavelNome.Should().BeNull();
        chamado.DataConclusao.Should().BeNull();
    }

    // ── AlterarPrioridade ────────────────────────────────────────────────────

    [Fact]
    public void AlterarPrioridade_DeveAtualizarDataLimite()
    {
        var chamado = CriarChamado(PrioridadeChamado.Baixa);
        var dataLimiteAnterior = chamado.DataLimite;

        chamado.AlterarPrioridade(PrioridadeChamado.Urgente);

        chamado.Prioridade.Should().Be(PrioridadeChamado.Urgente);
        chamado.DataLimite.Should().NotBe(dataLimiteAnterior);
    }

    // ── AtualizarDados ───────────────────────────────────────────────────────

    [Fact]
    public void AtualizarDados_DeveMudarTituloEDescricao()
    {
        var chamado = CriarChamado();

        chamado.AtualizarDados("Novo título", "Nova descrição");

        chamado.Titulo.Should().Be("Novo título");
        chamado.Descricao.Should().Be("Nova descrição");
    }

    [Fact]
    public void AtualizarDados_ComTituloVazio_DeveLancarArgumentException()
    {
        var chamado = CriarChamado();

        var act = () => chamado.AtualizarDados("", "Nova descrição");

        act.Should().Throw<ArgumentException>();
    }
}
