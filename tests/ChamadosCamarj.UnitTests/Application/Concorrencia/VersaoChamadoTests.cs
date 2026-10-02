using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using FluentAssertions;

namespace ChamadosCamarj.UnitTests.Application.Concorrencia;

/// <summary>Versão do chamado — spec correcoes-pre-deploy AC-09/AC-12, design §4.1.</summary>
public class VersaoChamadoTests
{
    [Fact]
    public void De_IgnoraFracaoAbaixoDeMicrossegundo()
    {
        // O valor em memória (UtcNow, 100ns) e o relido do Postgres (microssegundo) precisam dar a
        // mesma versão; senão a ação seguinte da mesma pessoa daria 409 falso (AC-12).
        var emMemoria = new DateTime(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc).AddTicks(1_234_567);
        var relidoDoBanco = new DateTime(emMemoria.Ticks / 10 * 10, DateTimeKind.Utc);

        VersaoChamado.De(emMemoria, DateTime.MinValue).Should().Be(VersaoChamado.De(relidoDoBanco, DateTime.MinValue));
    }

    [Fact]
    public void De_SemAtualizacao_UsaDataDeCriacao()
    {
        var criacao = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);
        var atualizacao = criacao.AddMinutes(5);

        VersaoChamado.De(null, criacao).Should().Be((criacao.Ticks / 10).ToString());
        VersaoChamado.De(atualizacao, criacao).Should().NotBe(VersaoChamado.De(null, criacao));
    }

    [Fact]
    public void ToResponse_PreencheAVersao()
    {
        var chamado = new Chamado("Título", "Descrição", "Ana", "ana@camarj.com.br", Guid.NewGuid(), Guid.NewGuid());

        var versaoAntes = chamado.ToResponse(incluirInternos: true).Versao;
        chamado.AlterarPrioridade(PrioridadeChamado.Alta);

        versaoAntes.Should().Be(VersaoChamado.De(null, chamado.DataCriacao));
        chamado.ToResponse(incluirInternos: true).Versao.Should().Be(VersaoChamado.De(chamado.DataAtualizacao, chamado.DataCriacao));
    }

    [Theory]
    [InlineData(AcaoChamado.Assumir, true)]
    [InlineData(AcaoChamado.Resolver, true)]
    [InlineData(AcaoChamado.Encerrar, true)]
    [InlineData(AcaoChamado.Reabrir, true)]
    [InlineData(AcaoChamado.AlterarStatus, true)]
    [InlineData(AcaoChamado.Cancelar, true)]
    [InlineData(AcaoChamado.Editar, true)]
    [InlineData(AcaoChamado.ReclassificarTipo, true)]
    [InlineData(AcaoChamado.Reatribuir, true)]
    [InlineData(AcaoChamado.AlterarPrioridade, true)]
    [InlineData(AcaoChamado.ForcarEncerramento, true)]
    [InlineData(AcaoChamado.Ver, false)]
    [InlineData(AcaoChamado.ComentarPublico, false)]
    [InlineData(AcaoChamado.ComentarInterno, false)]
    [InlineData(AcaoChamado.Anexar, false)]
    public void AlteraChamado_SoAsAcoesQueAlteram(AcaoChamado acao, bool esperado)
    {
        ChamadoPermissoes.AlteraChamado(acao).Should().Be(esperado);
    }

    [Fact]
    public void AlteraChamado_CobreTodasAsAcoesDoEnum()
    {
        // Ação nova no enum precisa entrar conscientemente numa das listas acima.
        Enum.GetValues<AcaoChamado>().Should().HaveCount(15);
    }
}
