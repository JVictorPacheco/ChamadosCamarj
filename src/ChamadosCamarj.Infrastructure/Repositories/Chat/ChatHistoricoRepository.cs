using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Repositories.Chat;

public class ChatHistoricoRepository : IChatHistoricoRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<ChatHistorico> _dbSet;

    public ChatHistoricoRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<ChatHistorico>();
    }

    public async Task AdicionarAsync(ChatHistorico historico, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(historico, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ChatHistorico>> ListarPorConversaAsync(Guid conversaId, CancellationToken cancellationToken)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(h => h.ConversaId == conversaId)
            .OrderByDescending(h => h.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ChatHistorico>> ListarTodasAsync(CancellationToken cancellationToken)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderByDescending(h => h.DataCriacao)
            .ToListAsync(cancellationToken);
    }
}
