using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Domain.Interfaces;

public interface IHistoricoRepository
{
    Task<HistoricoEntrada> AdicionarAsync(HistoricoEntrada historico, CancellationToken cancellationToken = default);
    Task<IEnumerable<HistoricoEntrada>> ObterPorChamadoAsync(Guid chamadoId, CancellationToken cancellationToken = default);
    Task<HistoricoEntrada?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
