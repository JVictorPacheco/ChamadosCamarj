using System.Collections.Concurrent;
using ChamadosCamarj.Application.Common;

namespace ChamadosCamarj.WebApi.Services;

/// <summary>
/// Lembra a última situação de prazo avisada de cada chamado, para o mesmo alerta não se repetir a
/// cada verificação (spec correcoes-pre-deploy AC-05). Extraído do SlaMonitorService para ser testável.
/// O aviso só conta como dado depois que o envio deu certo (review-2 R-02): se falhar, a próxima
/// verificação tenta de novo.
/// </summary>
public class SlaAlertasEnviados
{
    private readonly ConcurrentDictionary<Guid, SlaStatus> _ultimoAviso = new();

    /// <summary>Evento a enviar ("SlaAtencao"/"SlaAtrasado"), ou null se não há o que avisar. Não registra nada.</summary>
    public string? EventoANotificar(Guid chamadoId, SlaStatus status)
    {
        if (status == SlaStatus.DentroPrazo) return null;
        if (_ultimoAviso.TryGetValue(chamadoId, out var anterior) && anterior == status) return null;

        return status == SlaStatus.Atencao ? "SlaAtencao" : "SlaAtrasado";
    }

    /// <summary>Chamar depois que o alerta foi enviado com sucesso.</summary>
    public void RegistrarEnvio(Guid chamadoId, SlaStatus status) => _ultimoAviso[chamadoId] = status;

    /// <summary>Esquece os chamados que não estão mais em verificação (finalizados).</summary>
    public void ManterSo(IEnumerable<Guid> chamadosEmVerificacao)
    {
        var manter = chamadosEmVerificacao.ToHashSet();
        foreach (var id in _ultimoAviso.Keys.Where(id => !manter.Contains(id)).ToList())
            _ultimoAviso.TryRemove(id, out _);
    }
}
