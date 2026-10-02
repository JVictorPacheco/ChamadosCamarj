using ChamadosCamarj.Application.Common;
using ChamadosCamarj.WebApi.Services;
using FluentAssertions;

namespace ChamadosCamarj.UnitTests.WebApi.Services;

/// <summary>Spec correcoes-pre-deploy AC-05 (review R-01): o mesmo alerta de prazo não se repete.</summary>
public class SlaAlertasEnviadosTests
{
    private readonly SlaAlertasEnviados _enviados = new();
    private readonly Guid _chamado = Guid.NewGuid();

    [Fact]
    public void PrimeiraVezEmAtencao_Notifica()
    {
        _enviados.EventoANotificar(_chamado, SlaStatus.Atencao).Should().Be("SlaAtencao");
    }

    [Fact]
    public void MesmaSituacaoNaVerificacaoSeguinte_NaoNotificaDeNovo()
    {
        _enviados.EventoANotificar(_chamado, SlaStatus.Atrasado).Should().Be("SlaAtrasado");
        _enviados.EventoANotificar(_chamado, SlaStatus.Atrasado).Should().BeNull();
        _enviados.EventoANotificar(_chamado, SlaStatus.Atrasado).Should().BeNull();
    }

    [Fact]
    public void DeAtencaoParaAtrasado_NotificaAMudanca()
    {
        _enviados.EventoANotificar(_chamado, SlaStatus.Atencao);
        _enviados.EventoANotificar(_chamado, SlaStatus.Atrasado).Should().Be("SlaAtrasado");
    }

    [Fact]
    public void DentroDoPrazo_NuncaNotifica()
    {
        _enviados.EventoANotificar(_chamado, SlaStatus.DentroPrazo).Should().BeNull();
    }

    [Fact]
    public void ChamadoQueSaiuDaVerificacao_EhEsquecido()
    {
        // Chamado finalizado e depois reaberto atrasado volta a ser avisado.
        _enviados.EventoANotificar(_chamado, SlaStatus.Atrasado);
        _enviados.ManterSo([]);
        _enviados.EventoANotificar(_chamado, SlaStatus.Atrasado).Should().Be("SlaAtrasado");
    }
}
