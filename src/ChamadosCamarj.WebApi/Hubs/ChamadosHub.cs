using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace ChamadosCamarj.WebApi.Hubs;

/// <summary>
/// Hub SignalR para notificações em tempo real do sistema de chamados.
/// </summary>
public class ChamadosHub : Hub
{
    /// <summary>Atendentes e Admins: recebem avisos operacionais, como o de comentário interno.</summary>
    public const string GrupoAtendimento = "Atendimento";

    /// <summary>Só Admins: recebem os alertas de SLA de todos os chamados (spec correcoes-pre-deploy AC-03).</summary>
    public const string GrupoAdmins = "Admins";

    public override async Task OnConnectedAsync()
    {
        // Grupo padrão: todos os clientes recebem notificações globais
        await Groups.AddToGroupAsync(Context.ConnectionId, "Todos");

        // Grupos por perfil são definidos SÓ aqui, pelo servidor, a partir do token — o cliente
        // não escolhe em que grupo entra (spec correcoes-acesso-chamados AC-04).
        if (EhAtendimento(Context.User))
            await Groups.AddToGroupAsync(Context.ConnectionId, GrupoAtendimento);

        if (EhAdmin(Context.User))
            await Groups.AddToGroupAsync(Context.ConnectionId, GrupoAdmins);

        await base.OnConnectedAsync();
    }

    public static bool EhAtendimento(ClaimsPrincipal? usuario)
    {
        var perfil = usuario?.FindFirst("perfil")?.Value;
        return string.Equals(perfil, "Atendente", StringComparison.OrdinalIgnoreCase)
            || string.Equals(perfil, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    public static bool EhAdmin(ClaimsPrincipal? usuario) =>
        string.Equals(usuario?.FindFirst("perfil")?.Value, "Admin", StringComparison.OrdinalIgnoreCase);

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Todos");
        await base.OnDisconnectedAsync(exception);
    }
}
