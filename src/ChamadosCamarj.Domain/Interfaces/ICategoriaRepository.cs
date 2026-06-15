using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Domain.Interfaces;

public interface ICategoriaRepository
{
    Task<Categoria> AdicionarAsync(Categoria categoria, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Categoria categoria, CancellationToken cancellationToken = default);
    Task<Categoria?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Categoria>> ObterTodasAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Categoria>> ObterAtivasAsync(CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
}
