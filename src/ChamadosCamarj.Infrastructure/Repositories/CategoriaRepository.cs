using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<Categoria> _dbSet;

    public CategoriaRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<Categoria>();
    }

    public async Task<Categoria> AdicionarAsync(Categoria categoria, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(categoria, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return categoria;
    }

    public async Task AtualizarAsync(Categoria categoria, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(categoria);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Categoria?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Categoria>> ObterTodasAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().OrderBy(c => c.Nome).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Categoria>> ObterAtivasAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(c => c.Ativa).OrderBy(c => c.Nome).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.Id == id, cancellationToken);
    }
}
