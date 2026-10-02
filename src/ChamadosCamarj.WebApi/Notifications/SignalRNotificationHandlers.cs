using MediatR;
using Microsoft.AspNetCore.SignalR;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.WebApi.Hubs;

namespace ChamadosCamarj.WebApi.Notifications;

public class ChamadoCriadoNotificationHandler : INotificationHandler<ChamadoCriadoNotification>
{
    private readonly IHubContext<ChamadosHub> _hubContext;

    public ChamadoCriadoNotificationHandler(IHubContext<ChamadosHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(ChamadoCriadoNotification notification, CancellationToken cancellationToken)
    {
        // Vai para TODOS os conectados: só identificadores, nunca conteúdo (spec autorizacao-chamados
        // AC-19). Quem precisa dos dados rebusca pela API, que aplica as regras de acesso.
        await _hubContext.Clients.Group("Todos").SendAsync("ChamadoCriado", new
        {
            notification.ChamadoId,
            Status = notification.Status.ToString()
        }, cancellationToken);

        // Também dispara atualização de métricas
        await _hubContext.Clients.Group("Todos").SendAsync("MetricasAtualizadas", cancellationToken);
    }
}

public class StatusAlteradoNotificationHandler : INotificationHandler<StatusAlteradoNotification>
{
    private readonly IHubContext<ChamadosHub> _hubContext;

    public StatusAlteradoNotificationHandler(IHubContext<ChamadosHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(StatusAlteradoNotification notification, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group("Todos").SendAsync("StatusAlterado", new
        {
            notification.ChamadoId,
            NovoStatus = notification.NovoStatus,
            DataAtualizacao = notification.DataAtualizacao.ToString("O")
        }, cancellationToken);

        await _hubContext.Clients.Group("Todos").SendAsync("MetricasAtualizadas", cancellationToken);
    }
}

public class ComentarioAdicionadoNotificationHandler : INotificationHandler<ComentarioAdicionadoNotification>
{
    private readonly IHubContext<ChamadosHub> _hubContext;

    public ComentarioAdicionadoNotificationHandler(IHubContext<ChamadosHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(ComentarioAdicionadoNotification notification, CancellationToken cancellationToken)
    {
        // Sem autor nem texto: antes, todo usuário conectado recebia o conteúdo de qualquer
        // comentário, inclusive os internos (autorizacao-chamados AC-19). Comentário interno só
        // avisa o Atendimento — senão o próprio aviso revelaria ao Solicitante que existe um
        // interno (correcoes-acesso-chamados, review R-01).
        var destino = notification.Interno ? ChamadosHub.GrupoAtendimento : "Todos";
        await _hubContext.Clients.Group(destino).SendAsync("ComentarioAdicionado", new
        {
            notification.ChamadoId
        }, cancellationToken);
    }
}
