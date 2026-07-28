using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Repositories;

public class GrupoRepository : IGrupoRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<Grupo> _dbSet;

    public GrupoRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<Grupo>();
    }

    public async Task<Grupo?> ObterPorIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, ct);
    }

    public async Task<IEnumerable<Grupo>> ListarAsync(CancellationToken ct)
    {
        return await _dbSet.AsNoTracking().OrderBy(g => g.Nome).ToListAsync(ct);
    }

    public async Task AdicionarAsync(Grupo grupo, CancellationToken ct)
    {
        await _dbSet.AddAsync(grupo, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Grupo grupo, CancellationToken ct)
    {
        _dbSet.Update(grupo);
        await _context.SaveChangesAsync(ct);
    }
}
