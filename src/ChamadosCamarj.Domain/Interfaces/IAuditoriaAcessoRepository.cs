using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Domain.Interfaces;

public interface IAuditoriaAcessoRepository
{
    /// <summary>Grava várias linhas de uma vez (uma por item que mudou).</summary>
    Task AdicionarAsync(IEnumerable<AuditoriaAcesso> registros, CancellationToken cancellationToken);

    /// <summary>Mudanças de acesso de uma pessoa, mais recentes primeiro.</summary>
    Task<IEnumerable<AuditoriaAcesso>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken);
}
