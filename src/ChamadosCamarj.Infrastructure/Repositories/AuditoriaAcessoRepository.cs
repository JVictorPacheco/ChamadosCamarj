using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Repositories;

public class AuditoriaAcessoRepository : IAuditoriaAcessoRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<AuditoriaAcesso> _dbSet;

    public AuditoriaAcessoRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<AuditoriaAcesso>();
    }

    public async Task AdicionarAsync(IEnumerable<AuditoriaAcesso> registros, CancellationToken cancellationToken)
    {
        var lista = registros.ToList();
        if (lista.Count == 0) return;

        await _dbSet.AddRangeAsync(lista, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditoriaAcesso>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.UsuarioId == usuarioId)
            .OrderByDescending(a => a.DataCriacao)
            .ToListAsync(cancellationToken);
    }
}
