using Microsoft.EntityFrameworkCore;
using Npgsql;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Repositories.Chat;

public class ChatPresencaRepository : IChatPresencaRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<ChatPresenca> _dbSet;

    public ChatPresencaRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<ChatPresenca>();
    }

    public async Task<ChatPresenca?> ObterPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId, cancellationToken);
    }

    public async Task<IEnumerable<ChatPresenca>> ListarTodasAsync(CancellationToken cancellationToken)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task AdicionarOuAtualizarAsync(ChatPresenca presenca, CancellationToken cancellationToken)
    {
        var existente = await _dbSet.FirstOrDefaultAsync(p => p.UsuarioId == presenca.UsuarioId, cancellationToken);
        if (existente is null)
        {
            await _dbSet.AddAsync(presenca, cancellationToken);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // Heartbeat concorrente: outra requisição simultânea já inseriu a presença deste
                // usuário (corrida entre 2 chamadas quase ao mesmo tempo, ex: StrictMode ou múltiplas
                // abas). Tratamos como no-op — o estado desejado (usuário Online, heartbeat recente)
                // já está persistido pela outra requisição.
                _context.Entry(presenca).State = EntityState.Detached;
            }
            return;
        }

        _dbSet.Update(presenca);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ChatPresenca>> ListarParaMarcarAusenteAsync(DateTime limite, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(p => p.Status == StatusPresenca.Online && p.UltimoHeartbeat < limite)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ChatPresenca>> ListarParaMarcarOfflineAsync(DateTime limite, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(p => p.Status != StatusPresenca.Offline && p.UltimoHeartbeat < limite)
            .ToListAsync(cancellationToken);
    }

    public async Task AtualizarVariasAsync(IEnumerable<ChatPresenca> presencas, CancellationToken cancellationToken)
    {
        _dbSet.UpdateRange(presencas);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
