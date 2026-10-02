using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Domain.Interfaces;

public interface ITipoChamadoRepository
{
    Task<TipoChamado> AdicionarAsync(TipoChamado tipo, CancellationToken cancellationToken = default);
    Task AtualizarAsync(TipoChamado tipo, CancellationToken cancellationToken = default);
    Task<TipoChamado?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TipoChamado>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TipoChamado>> ObterAtivosAsync(CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
    Task RemoverAsync(TipoChamado tipo, CancellationToken cancellationToken = default);
    Task<bool> PossuiChamadosAsync(Guid id, CancellationToken cancellationToken = default);
}
