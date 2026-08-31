using MediatR;
using Microsoft.AspNetCore.SignalR;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.WebApi.Hubs;

namespace ChamadosCamarj.WebApi.Notifications;

public class ChatNovaMensagemNotificationHandler : INotificationHandler<ChatNovaMensagemNotification>
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatNovaMensagemNotificationHandler(IHubContext<ChatHub> hubContext) => _hubContext = hubContext;

    public Task Handle(ChatNovaMensagemNotification notification, CancellationToken cancellationToken) =>
        _hubContext.Clients.Group(ChatHub.GrupoConversa(notification.ConversaId))
            .SendAsync("NovaMensagem", notification.Mensagem, cancellationToken);
}

public class ChatMensagemEditadaNotificationHandler : INotificationHandler<ChatMensagemEditadaNotification>
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatMensagemEditadaNotificationHandler(IHubContext<ChatHub> hubContext) => _hubContext = hubContext;

    public Task Handle(ChatMensagemEditadaNotification notification, CancellationToken cancellationToken) =>
        _hubContext.Clients.Group(ChatHub.GrupoConversa(notification.ConversaId))
            .SendAsync("MensagemEditada", notification.Mensagem, cancellationToken);
}

public class ChatMensagemDeletadaNotificationHandler : INotificationHandler<ChatMensagemDeletadaNotification>
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatMensagemDeletadaNotificationHandler(IHubContext<ChatHub> hubContext) => _hubContext = hubContext;

    public Task Handle(ChatMensagemDeletadaNotification notification, CancellationToken cancellationToken) =>
        _hubContext.Clients.Group(ChatHub.GrupoConversa(notification.ConversaId))
            .SendAsync("MensagemDeletada", notification.MensagemId, cancellationToken);
}

public class ChatReacaoAtualizadaNotificationHandler : INotificationHandler<ChatReacaoAtualizadaNotification>
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatReacaoAtualizadaNotificationHandler(IHubContext<ChatHub> hubContext) => _hubContext = hubContext;

    public Task Handle(ChatReacaoAtualizadaNotification notification, CancellationToken cancellationToken) =>
        _hubContext.Clients.Group(ChatHub.GrupoConversa(notification.ConversaId))
            .SendAsync("ReacaoAtualizada", notification.MensagemId, notification.Reacoes, cancellationToken);
}

public class ChatPresencaAtualizadaNotificationHandler : INotificationHandler<ChatPresencaAtualizadaNotification>
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatPresencaAtualizadaNotificationHandler(IHubContext<ChatHub> hubContext) => _hubContext = hubContext;

    public Task Handle(ChatPresencaAtualizadaNotification notification, CancellationToken cancellationToken) =>
        _hubContext.Clients.Group(ChatHub.GrupoPresencaGlobal)
            .SendAsync("PresencaAtualizada", new
            {
                usuarioId = notification.UsuarioId,
                usuarioNome = notification.UsuarioNome,
                status = notification.Status
            }, cancellationToken);
}

public class ChatAcessoRevogadoNotificationHandler : INotificationHandler<ChatAcessoRevogadoNotification>
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatAcessoRevogadoNotificationHandler(IHubContext<ChatHub> hubContext) => _hubContext = hubContext;

    public Task Handle(ChatAcessoRevogadoNotification notification, CancellationToken cancellationToken) =>
        _hubContext.Clients.User(notification.UsuarioId.ToString())
            .SendAsync("AcessoRevogado", cancellationToken);
}

public class ChatMensagemLidaNotificationHandler : INotificationHandler<ChatMensagemLidaNotification>
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatMensagemLidaNotificationHandler(IHubContext<ChatHub> hubContext) => _hubContext = hubContext;

    public Task Handle(ChatMensagemLidaNotification notification, CancellationToken cancellationToken) =>
        _hubContext.Clients.Group(ChatHub.GrupoConversa(notification.ConversaId))
            .SendAsync("MensagemLida", notification.ConversaId, notification.UsuarioId, notification.LeituraEm, cancellationToken);
}

public class ChatNovaConversaNotificationHandler : INotificationHandler<ChatNovaConversaNotification>
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatNovaConversaNotificationHandler(IHubContext<ChatHub> hubContext) => _hubContext = hubContext;

    public async Task Handle(ChatNovaConversaNotification notification, CancellationToken cancellationToken)
    {
        foreach (var participanteId in notification.ParticipanteIds)
        {
            await _hubContext.Clients.User(participanteId.ToString())
                .SendAsync("NovaConversa", notification.Conversa, cancellationToken);
        }
    }
}
