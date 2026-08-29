using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Domain.Interfaces;

public interface IChatConversaRepository
{
    Task<ChatConversa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<ChatConversa>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken);
    Task<ChatConversa?> ObterPrivadaEntreUsuariosAsync(Guid usuarioAId, Guid usuarioBId, CancellationToken cancellationToken);
    Task<ChatParticipante?> ObterParticipanteAsync(Guid conversaId, Guid usuarioId, CancellationToken cancellationToken);
    Task<IEnumerable<ChatConversa>> ListarConversasComUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken);
    Task AdicionarAsync(ChatConversa conversa, CancellationToken cancellationToken);
    Task AtualizarAsync(ChatConversa conversa, CancellationToken cancellationToken);
    Task AtualizarParticipanteAsync(ChatParticipante participante, CancellationToken cancellationToken);
}
