using Microsoft.AspNetCore.SignalR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.WebApi.Hubs;

namespace ChamadosCamarj.WebApi.Services;

/// <summary>
/// Envia um alerta de SLA só para quem pode ver o chamado (spec correcoes-pre-deploy AC-01..04):
/// todos os Admins (grupo do hub) e os Atendentes que a regra única de visibilidade deixa ver.
/// Solicitante nunca recebe. Separado do SlaMonitorService para o envio ser testável.
/// </summary>
public class SlaAlertaNotificador
{
    private readonly IHubContext<ChamadosHub> _hubContext;
    private readonly IChamadoRepository _chamadoRepository;

    public SlaAlertaNotificador(IHubContext<ChamadosHub> hubContext, IChamadoRepository chamadoRepository)
    {
        _hubContext = hubContext;
        _chamadoRepository = chamadoRepository;
    }

    public async Task NotificarAsync(Guid chamadoId, int numero, string evento, string mensagem, CancellationToken cancellationToken)
    {
        var payload = new { chamadoId = chamadoId.ToString(), numero, mensagem };

        await _hubContext.Clients.Group(ChamadosHub.GrupoAdmins).SendAsync(evento, payload, cancellationToken);

        var atendentes = await _chamadoRepository.ListarAtendentesQuePodemVerAsync(chamadoId, cancellationToken);
        if (atendentes.Count > 0)
            await _hubContext.Clients.Users(atendentes.Select(id => id.ToString()).ToList())
                .SendAsync(evento, payload, cancellationToken);
    }
}
