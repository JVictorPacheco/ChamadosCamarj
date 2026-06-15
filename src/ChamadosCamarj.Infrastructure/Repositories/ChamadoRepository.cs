using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Repositories;

public class ChamadoRepository : IChamadoRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<Chamado> _dbSet;

    public ChamadoRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<Chamado>();
    }

    public async Task<Chamado> AdicionarAsync(Chamado chamado, CancellationToken cancellationToken = default)
    {
        if (chamado == null)
            throw new ArgumentNullException(nameof(chamado));

        await _dbSet.AddAsync(chamado, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return chamado;
    }

    public async Task AtualizarAsync(Chamado chamado, CancellationToken cancellationToken = default)
    {
        if (chamado == null)
            throw new ArgumentNullException(nameof(chamado));

        _dbSet.Update(chamado);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Chamado?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Categoria)
            .Include(c => c.Comentarios)
            .Include(c => c.Anexos)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Chamado>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Categoria)
            .Include(c => c.Comentarios)
            .Include(c => c.Anexos)
            .AsNoTracking()
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Chamado>> ObterPorStatusAsync(Domain.Enums.StatusChamado status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Categoria)
            .AsNoTracking()
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Chamado>> ObterPorSolicitanteAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Categoria)
            .AsNoTracking()
            .Where(c => c.SolicitanteEmail == email)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Chamado>> ObterPorResponsavelAsync(Guid responsavelId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Categoria)
            .AsNoTracking()
            .Where(c => c.ResponsavelId == responsavelId)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Chamado>> ObterAtrasadosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Categoria)
            .AsNoTracking()
            .Where(c => c.DataLimite != null && c.DataLimite < DateTime.UtcNow && c.Status != Domain.Enums.StatusChamado.Resolvido && c.Status != Domain.Enums.StatusChamado.Fechado && c.Status != Domain.Enums.StatusChamado.Cancelado)
            .OrderBy(c => c.DataLimite)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.Id == id, cancellationToken);
    }
}
