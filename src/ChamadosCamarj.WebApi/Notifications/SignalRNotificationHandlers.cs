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
        await _hubContext.Clients.Group("Todos").SendAsync("ChamadoCriado", new
        {
            notification.ChamadoId,
            notification.Titulo,
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
        await _hubContext.Clients.Group("Todos").SendAsync("ComentarioAdicionado", new
        {
            notification.ChamadoId,
            notification.Autor,
            notification.Conteudo
        }, cancellationToken);
    }
}
