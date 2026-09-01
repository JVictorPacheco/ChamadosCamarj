using MediatR;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Common.Notifications;

/// <summary>
/// Notificações de chat despachadas via MediatR e traduzidas para o SignalR ChatHub
/// pelos handlers em WebApi (mesma separação de camadas do padrão de chamados).
/// </summary>

// review-fase9-independente.md #3: DestinatarioIds é opcional — quando o publisher já tem a
// conversa (com participantes) carregada em mãos (ex: DefinirChatPerfilCommandHandler, que itera
// várias conversas de um usuário), evita o handler refazer o mesmo SELECT que o publisher já pagou.
// Quando null, o handler busca a conversa ele mesmo (caso comum: EnviarMensagem/EnviarArquivo, que
// só tinham o participante isolado carregado, não a conversa inteira).
public record ChatNovaMensagemNotification(Guid ConversaId, object Mensagem, IEnumerable<Guid>? DestinatarioIds = null) : INotification;

public record ChatMensagemEditadaNotification(Guid ConversaId, object Mensagem) : INotification;

public record ChatMensagemDeletadaNotification(Guid ConversaId, Guid MensagemId) : INotification;

public record ChatReacaoAtualizadaNotification(Guid ConversaId, Guid MensagemId, object Reacoes) : INotification;

public record ChatPresencaAtualizadaNotification(Guid UsuarioId, string UsuarioNome, string Status) : INotification;

public record ChatAcessoRevogadoNotification(Guid UsuarioId) : INotification;

public record ChatMensagemLidaNotification(Guid ConversaId, Guid UsuarioId, DateTime LeituraEm) : INotification;

public record ChatNovaConversaNotification(Guid ConversaId, IEnumerable<Guid> ParticipanteIds, object Conversa) : INotification;

public record ChatParticipanteAdicionadoNotification(Guid ConversaId, Guid UsuarioId) : INotification;

public record ChatParticipanteRemovidoNotification(Guid ConversaId, Guid UsuarioId) : INotification;

// AC-47: avisa o usuário afetado que o ChatPerfil dele mudou — publicado via ChamadosHub (canal
// global, sempre conectado pra qualquer usuário logado), não via ChatHub (só conectado na tela de
// chat, que a própria pessoa pode não conseguir acessar se acabou de perder o acesso).
public record ChatPerfilAtualizadoNotification(Guid UsuarioId, ChatPerfil NovoChatPerfil) : INotification;
