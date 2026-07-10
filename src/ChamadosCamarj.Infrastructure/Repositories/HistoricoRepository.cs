using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Repositories;

public class HistoricoRepository : IHistoricoRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<HistoricoEntrada> _dbSet;

    public HistoricoRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<HistoricoEntrada>();
    }

    public async Task<HistoricoEntrada> AdicionarAsync(HistoricoEntrada historico, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(historico, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return historico;
    }

    public async Task<IEnumerable<HistoricoEntrada>> ObterPorChamadoAsync(Guid chamadoId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(h => h.ChamadoId == chamadoId)
            .OrderByDescending(h => h.DataHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<HistoricoEntrada?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }
}
