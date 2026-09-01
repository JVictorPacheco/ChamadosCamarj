using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Domain.Interfaces;

public interface IChatMensagemRepository
{
    Task<ChatMensagem?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ChatMensagem?> ObterPorIdComReacoesAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<ChatMensagem>> ListarPorConversaAsync(Guid conversaId, int pagina, int tamanhoPagina, CancellationToken cancellationToken);
    Task<int> ContarPorConversaAsync(Guid conversaId, CancellationToken cancellationToken);
    Task<ChatMensagem?> ObterUltimaPorConversaAsync(Guid conversaId, CancellationToken cancellationToken);
    Task<int> ContarNaoLidasAsync(Guid conversaId, Guid usuarioId, DateTime? ultimaLeituraEm, CancellationToken cancellationToken);
    Task<Dictionary<Guid, ChatMensagem>> ObterUltimasMensagensPorConversasAsync(IEnumerable<Guid> conversaIds, CancellationToken cancellationToken);
    Task<Dictionary<Guid, string?>> ObterConteudosPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    Task<Dictionary<Guid, int>> ContarNaoLidasPorConversasAsync(IEnumerable<(Guid ConversaId, DateTime? UltimaLeituraEm)> conversas, Guid usuarioId, CancellationToken cancellationToken);
    Task AdicionarAsync(ChatMensagem mensagem, CancellationToken cancellationToken);
    Task AtualizarAsync(ChatMensagem mensagem, CancellationToken cancellationToken);
    Task<ChatMensagemReacao?> ObterReacaoAsync(Guid mensagemId, Guid usuarioId, string emoji, CancellationToken cancellationToken);
    Task AdicionarReacaoAsync(ChatMensagemReacao reacao, CancellationToken cancellationToken);
    Task RemoverReacaoAsync(ChatMensagemReacao reacao, CancellationToken cancellationToken);
}
