using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Domain.Interfaces;

public interface IChatHistoricoRepository
{
    Task AdicionarAsync(ChatHistorico historico, CancellationToken cancellationToken);
    Task<IEnumerable<ChatHistorico>> ListarPorConversaAsync(Guid conversaId, CancellationToken cancellationToken);
    Task<IEnumerable<ChatHistorico>> ListarTodasAsync(CancellationToken cancellationToken);
}
