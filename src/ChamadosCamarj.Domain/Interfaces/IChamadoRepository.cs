using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Domain.Interfaces;

public interface IChamadoRepository
{
    // Comandos
    Task<Chamado> AdicionarAsync(Chamado chamado, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Chamado chamado, CancellationToken cancellationToken = default);

    // Consultas
    Task<Chamado?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterPorStatusAsync(Enums.StatusChamado status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterPorSolicitanteAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterPorResponsavelAsync(Guid responsavelId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterAtrasadosAsync(CancellationToken cancellationToken = default);

    // Verificações
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
}
