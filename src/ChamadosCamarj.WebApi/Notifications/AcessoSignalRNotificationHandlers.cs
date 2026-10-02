using MediatR;
using Microsoft.AspNetCore.SignalR;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.WebApi.Hubs;

namespace ChamadosCamarj.WebApi.Notifications;

/// <summary>
/// Avisa só a pessoa cujos acessos mudaram, para o menu dela se atualizar na hora (spec controle-de-acesso
/// AC-11/AC-13). Mesmo mecanismo do ChatPerfilAtualizado.
/// </summary>
public class AcessosAtualizadosNotificationHandler : INotificationHandler<AcessosAtualizadosNotification>
{
    private readonly IHubContext<ChamadosHub> _hubContext;

    public AcessosAtualizadosNotificationHandler(IHubContext<ChamadosHub> hubContext) => _hubContext = hubContext;

    public Task Handle(AcessosAtualizadosNotification notification, CancellationToken cancellationToken) =>
        _hubContext.Clients.User(notification.UsuarioId.ToString())
            .SendAsync("AcessosAtualizados", new
            {
                modulos = notification.Modulos,
                chatPerfil = notification.ChatPerfil.ToString(),
            }, cancellationToken);
}
