using MediatR;
using Microsoft.AspNetCore.SignalR;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.WebApi.Hubs;

namespace ChamadosCamarj.WebApi.Notifications;

public class ChatNovaMensagemNotificationHandler : INotificationHandler<ChatNovaMensagemNotification>
{
    private readonly IHubContext<ChatHub> _chatHubContext;
    private readonly IHubContext<ChamadosHub> _chamadosHubContext;
    private readonly IChatConversaRepository _conversaRepository;
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly ILogger<ChatNovaMensagemNotificationHandler> _logger;

    public ChatNovaMensagemNotificationHandler(
        IHubContext<ChatHub> chatHubContext,
        IHubContext<ChamadosHub> chamadosHubContext,
        IChatConversaRepository conversaRepository,
        IUsuarioPerfilRepository usuarioPerfilRepository,
        ILogger<ChatNovaMensagemNotificationHandler> logger)
    {
        _chatHubContext = chatHubContext;
        _chamadosHubContext = chamadosHubContext;
        _conversaRepository = conversaRepository;
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _logger = logger;
    }

    public async Task Handle(ChatNovaMensagemNotification notification, CancellationToken cancellationToken)
    {
        await _chatHubContext.Clients.Group(ChatHub.GrupoConversa(notification.ConversaId))
            .SendAsync("NovaMensagem", notification.Mensagem, cancellationToken);

        // Bug #10: quem não está na tela /chat não tem conexão no ChatHub (só existe lá), então
        // nunca sabia que chegou mensagem nova — o badge de não lidas na sidebar só atualizava ao
        // entrar no chat. Mesmo motivo/solução do ChatPerfilAtualizado: manda pelo ChamadosHub
        // (conexão global) pra cada participante ativo, exceto quem enviou a mensagem.
        //
        // Tudo isso é best-effort: a mensagem já foi persistida antes deste handler rodar (ver
        // EnviarMensagemCommandHandler/EnviarArquivoCommandHandler), então uma falha aqui (banco
        // fora do ar, hub indisponível) não pode subir e derrubar um comando que já teve sucesso —
        // achado #3 da review-fase9-independente.md: isso já causou um 500 falso com a mensagem
        // salva e o aviso de retry do AC-52 aparecendo por engano.
        try
        {
            if (notification.Mensagem is not ChatMensagemResponse mensagem) return;

            List<Guid> participantesIds;
            if (notification.DestinatarioIds is not null)
            {
                // Publisher já tinha a conversa (com participantes) carregada — evita refazer o
                // mesmo SELECT aqui (achado #3 da review-fase9-independente.md, N+1 confirmado em
                // DefinirChatPerfilCommandHandler, que publica uma notificação por conversa).
                participantesIds = notification.DestinatarioIds
                    .Where(id => id != mensagem.AutorId)
                    .ToList();
            }
            else
            {
                var conversa = await _conversaRepository.ObterPorIdAsync(notification.ConversaId, cancellationToken);
                if (conversa is null) return;

                participantesIds = conversa.Participantes
                    .Where(p => p.Ativo && p.UsuarioId != mensagem.AutorId)
                    .Select(p => p.UsuarioId)
                    .ToList();
            }
            if (participantesIds.Count == 0) return;

            // Achado #2 da review-fase9-independente.md: revogar acesso ao chat não remove o vínculo
            // de participante (só bloqueia a tela) — sem este filtro, alguém com ChatPerfil =
            // SemAcesso continuaria recebendo esse evento e sendo empurrado a recarregar a lista de
            // conversas a cada mensagem nova em qualquer conversa da qual ainda é membro.
            var usuarios = await _usuarioPerfilRepository.ListarPorIdsAsync(participantesIds, cancellationToken);
            var destinatarios = usuarios
                .Where(u => u.ChatPerfil != ChatPerfil.SemAcesso)
                .Select(u => u.Id.ToString());

            await Task.WhenAll(destinatarios.Select(id =>
                _chamadosHubContext.Clients.User(id).SendAsync("ChatConversaAtualizada", cancellationToken)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao notificar ChatConversaAtualizada para a conversa {ConversaId}.", notification.ConversaId);
        }
    }
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

public class ChatParticipanteAdicionadoNotificationHandler : INotificationHandler<ChatParticipanteAdicionadoNotification>
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatParticipanteAdicionadoNotificationHandler(IHubContext<ChatHub> hubContext) => _hubContext = hubContext;

    public Task Handle(ChatParticipanteAdicionadoNotification notification, CancellationToken cancellationToken)
    {
        // Notifica o grupo (pra quem já está na conversa aberta atualizar a lista) e o novo
        // participante especificamente (pra "NovaConversa" aparecer pra ele, que ainda não
        // tinha essa conversa na lista).
        var paraGrupo = _hubContext.Clients.Group(ChatHub.GrupoConversa(notification.ConversaId))
            .SendAsync("ParticipanteAdicionado", notification.ConversaId, cancellationToken);
        var paraNovoParticipante = _hubContext.Clients.User(notification.UsuarioId.ToString())
            .SendAsync("ParticipanteAdicionado", notification.ConversaId, cancellationToken);
        return Task.WhenAll(paraGrupo, paraNovoParticipante);
    }
}

public class ChatParticipanteRemovidoNotificationHandler : INotificationHandler<ChatParticipanteRemovidoNotification>
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatParticipanteRemovidoNotificationHandler(IHubContext<ChatHub> hubContext) => _hubContext = hubContext;

    public Task Handle(ChatParticipanteRemovidoNotification notification, CancellationToken cancellationToken) =>
        _hubContext.Clients.Group(ChatHub.GrupoConversa(notification.ConversaId))
            .SendAsync("ParticipanteRemovido", notification.ConversaId, cancellationToken);
}

// Publica via ChamadosHub (não ChatHub) de propósito — ver comentário em ChatPerfilAtualizadoNotification.
public class ChatPerfilAtualizadoNotificationHandler : INotificationHandler<ChatPerfilAtualizadoNotification>
{
    private readonly IHubContext<ChamadosHub> _hubContext;

    public ChatPerfilAtualizadoNotificationHandler(IHubContext<ChamadosHub> hubContext) => _hubContext = hubContext;

    public Task Handle(ChatPerfilAtualizadoNotification notification, CancellationToken cancellationToken) =>
        _hubContext.Clients.User(notification.UsuarioId.ToString())
            .SendAsync("ChatPerfilAtualizado", new { chatPerfil = notification.NovoChatPerfil.ToString() }, cancellationToken);
}
