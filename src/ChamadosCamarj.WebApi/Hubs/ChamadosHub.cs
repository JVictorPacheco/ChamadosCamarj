using Microsoft.AspNetCore.SignalR;

namespace ChamadosCamarj.WebApi.Hubs;

/// <summary>
/// Hub SignalR para notificações em tempo real do sistema de chamados.
/// </summary>
public class ChamadosHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Grupo padrão: todos os clientes recebem notificações globais
        await Groups.AddToGroupAsync(Context.ConnectionId, "Todos");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Todos");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Permite que o cliente entre em um grupo específico (ex: perfil "Atendente").
    /// </summary>
    public async Task EntrarGrupo(string grupo)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, grupo);
    }

    /// <summary>
    /// Permite que o cliente saia de um grupo específico.
    /// </summary>
    public async Task SairGrupo(string grupo)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, grupo);
    }
}
