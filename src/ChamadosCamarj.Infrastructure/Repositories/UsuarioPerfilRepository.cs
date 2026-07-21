using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Repositories;

public class UsuarioPerfilRepository : IUsuarioPerfilRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<UsuarioPerfil> _dbSet;

    public UsuarioPerfilRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<UsuarioPerfil>();
    }

    public async Task<UsuarioPerfil?> ObterPorEmailAsync(string email, CancellationToken ct)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<UsuarioPerfil?> ObterPorIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<IEnumerable<UsuarioPerfil>> ListarAsync(CancellationToken ct)
    {
        return await _dbSet.AsNoTracking().OrderBy(u => u.Nome).ToListAsync(ct);
    }

    public async Task AdicionarAsync(UsuarioPerfil usuario, CancellationToken ct)
    {
        await _dbSet.AddAsync(usuario, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(UsuarioPerfil usuario, CancellationToken ct)
    {
        _dbSet.Update(usuario);
        await _context.SaveChangesAsync(ct);
    }
}
