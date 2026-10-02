using ChamadosCamarj.Application.Common;
using ChamadosCamarj.WebApi.Services;
using FluentAssertions;

namespace ChamadosCamarj.UnitTests.WebApi.Services;

/// <summary>Spec correcoes-pre-deploy AC-05 (review R-01; review-2 R-02): o mesmo alerta de prazo não se repete.</summary>
public class SlaAlertasEnviadosTests
{
    private readonly SlaAlertasEnviados _enviados = new();
    private readonly Guid _chamado = Guid.NewGuid();

    // Simula o monitor: decide, envia (com sucesso) e registra.
    private string? VerificarEEnviar(SlaStatus status)
    {
        var evento = _enviados.EventoANotificar(_chamado, status);
        if (evento is not null) _enviados.RegistrarEnvio(_chamado, status);
        return evento;
    }

    [Fact]
    public void PrimeiraVezEmAtencao_Notifica()
    {
        VerificarEEnviar(SlaStatus.Atencao).Should().Be("SlaAtencao");
    }

    [Fact]
    public void MesmaSituacaoNaVerificacaoSeguinte_NaoNotificaDeNovo()
    {
        VerificarEEnviar(SlaStatus.Atrasado).Should().Be("SlaAtrasado");
        VerificarEEnviar(SlaStatus.Atrasado).Should().BeNull();
        VerificarEEnviar(SlaStatus.Atrasado).Should().BeNull();
    }

    [Fact]
    public void DeAtencaoParaAtrasado_NotificaAMudanca()
    {
        VerificarEEnviar(SlaStatus.Atencao);
        VerificarEEnviar(SlaStatus.Atrasado).Should().Be("SlaAtrasado");
    }

    [Fact]
    public void DentroDoPrazo_NuncaNotifica()
    {
        VerificarEEnviar(SlaStatus.DentroPrazo).Should().BeNull();
    }

    [Fact]
    public void ChamadoQueSaiuDaVerificacao_EhEsquecido()
    {
        // Chamado finalizado e depois reaberto atrasado volta a ser avisado.
        VerificarEEnviar(SlaStatus.Atrasado);
        _enviados.ManterSo([]);
        VerificarEEnviar(SlaStatus.Atrasado).Should().Be("SlaAtrasado");
    }

    [Fact]
    public void EnvioQueFalhou_TentaDeNovoNaProximaVerificacao()
    {
        // review-2 R-02: decidiu avisar, mas o envio falhou (RegistrarEnvio não foi chamado).
        _enviados.EventoANotificar(_chamado, SlaStatus.Atrasado).Should().Be("SlaAtrasado");
        _enviados.EventoANotificar(_chamado, SlaStatus.Atrasado).Should().Be("SlaAtrasado");
    }
}
