using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Domain.Interfaces;

public interface IGrupoRepository
{
    Task<Grupo?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<Grupo>> ListarAsync(CancellationToken ct);
    Task AdicionarAsync(Grupo grupo, CancellationToken ct);
    Task AtualizarAsync(Grupo grupo, CancellationToken ct);
}
