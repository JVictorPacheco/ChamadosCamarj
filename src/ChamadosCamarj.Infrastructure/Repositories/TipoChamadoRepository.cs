using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Repositories;

public class TipoChamadoRepository : ITipoChamadoRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<TipoChamado> _dbSet;

    public TipoChamadoRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TipoChamado>();
    }

    public async Task<TipoChamado> AdicionarAsync(TipoChamado tipo, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(tipo, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return tipo;
    }

    public async Task AtualizarAsync(TipoChamado tipo, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(tipo);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TipoChamado?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TipoChamado>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().OrderBy(c => c.Nome).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TipoChamado>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(c => c.Ativo).OrderBy(c => c.Nome).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task RemoverAsync(TipoChamado tipo, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(tipo);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> PossuiChamadosAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Chamados.AnyAsync(c => c.TipoId == id, cancellationToken);
    }
}
