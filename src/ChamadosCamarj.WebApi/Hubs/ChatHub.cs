using Microsoft.AspNetCore.SignalR;

namespace ChamadosCamarj.WebApi.Hubs;

/// <summary>
/// Hub SignalR dedicado ao chat corporativo. Separado do ChamadosHub por ter grupos e
/// escopo próprios: um grupo por conversa ("chat-{conversaId}") e um grupo global de
/// presença ("presenca-global").
/// </summary>
public class ChatHub : Hub
{
    public const string GrupoPresencaGlobal = "presenca-global";

    public static string GrupoConversa(Guid conversaId) => $"chat-{conversaId}";

    public override async Task OnConnectedAsync()
    {
        // Todo cliente conectado acompanha a presença global (visível a qualquer perfil).
        await Groups.AddToGroupAsync(Context.ConnectionId, GrupoPresencaGlobal);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GrupoPresencaGlobal);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Cliente entra no grupo de uma conversa para receber mensagens/eventos em tempo real.
    /// </summary>
    public async Task EntrarConversa(Guid conversaId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GrupoConversa(conversaId));
    }

    /// <summary>
    /// Cliente sai do grupo de uma conversa.
    /// </summary>
    public async Task SairConversa(Guid conversaId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GrupoConversa(conversaId));
    }

    /// <summary>
    /// Indica que o usuário começou a digitar em uma conversa.
    /// </summary>
    public async Task Digitando(Guid conversaId, string usuarioNome)
    {
        await Clients.OthersInGroup(GrupoConversa(conversaId))
            .SendAsync("DigitandoIniciou", conversaId, usuarioNome);
    }

    /// <summary>
    /// Indica que o usuário parou de digitar em uma conversa.
    /// </summary>
    public async Task PararDigitar(Guid conversaId)
    {
        await Clients.OthersInGroup(GrupoConversa(conversaId))
            .SendAsync("DigitandoParou", conversaId);
    }
}
