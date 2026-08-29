using MediatR;

namespace ChamadosCamarj.Application.Common.Notifications;

/// <summary>
/// Notificações de chat despachadas via MediatR e traduzidas para o SignalR ChatHub
/// pelos handlers em WebApi (mesma separação de camadas do padrão de chamados).
/// </summary>

public record ChatNovaMensagemNotification(Guid ConversaId, object Mensagem) : INotification;

public record ChatMensagemEditadaNotification(Guid ConversaId, object Mensagem) : INotification;

public record ChatMensagemDeletadaNotification(Guid ConversaId, Guid MensagemId) : INotification;

public record ChatReacaoAtualizadaNotification(Guid ConversaId, Guid MensagemId, object Reacoes) : INotification;

public record ChatPresencaAtualizadaNotification(Guid UsuarioId, string UsuarioNome, string Status) : INotification;

public record ChatAcessoRevogadoNotification(Guid UsuarioId) : INotification;

public record ChatMensagemLidaNotification(Guid ConversaId, Guid UsuarioId, DateTime LeituraEm) : INotification;

public record ChatNovaConversaNotification(Guid ConversaId, IEnumerable<Guid> ParticipanteIds, object Conversa) : INotification;
