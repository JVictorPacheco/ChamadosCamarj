using Microsoft.EntityFrameworkCore;
using Npgsql;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Repositories.Chat;

public class ChatMensagemRepository : IChatMensagemRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<ChatMensagem> _dbSet;

    public ChatMensagemRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<ChatMensagem>();
    }

    public async Task<ChatMensagem?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<ChatMensagem?> ObterPorIdComReacoesAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.Reacoes)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ChatMensagem>> ListarPorConversaAsync(Guid conversaId, int pagina, int tamanhoPagina, CancellationToken cancellationToken)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(m => m.Reacoes)
            .Where(m => m.ConversaId == conversaId)
            .OrderByDescending(m => m.DataCriacao)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ContarPorConversaAsync(Guid conversaId, CancellationToken cancellationToken)
    {
        return await _dbSet
            .AsNoTracking()
            .CountAsync(m => m.ConversaId == conversaId, cancellationToken);
    }

    public async Task<ChatMensagem?> ObterUltimaPorConversaAsync(Guid conversaId, CancellationToken cancellationToken)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(m => m.ConversaId == conversaId)
            .OrderByDescending(m => m.DataCriacao)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> ContarNaoLidasAsync(Guid conversaId, Guid usuarioId, DateTime? ultimaLeituraEm, CancellationToken cancellationToken)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(m => m.ConversaId == conversaId && m.AutorId != usuarioId);

        if (ultimaLeituraEm.HasValue)
            query = query.Where(m => m.DataCriacao > ultimaLeituraEm.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, ChatMensagem>> ObterUltimasMensagensPorConversasAsync(IEnumerable<Guid> conversaIds, CancellationToken cancellationToken)
    {
        var ids = conversaIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, ChatMensagem>();

        var ultimas = await _dbSet
            .AsNoTracking()
            .Where(m => ids.Contains(m.ConversaId))
            .GroupBy(m => m.ConversaId)
            .Select(g => g.OrderByDescending(m => m.DataCriacao).First())
            .ToListAsync(cancellationToken);

        return ultimas.ToDictionary(m => m.ConversaId);
    }

    public async Task<Dictionary<Guid, int>> ContarNaoLidasPorConversasAsync(
        IEnumerable<(Guid ConversaId, DateTime? UltimaLeituraEm)> conversas, Guid usuarioId, CancellationToken cancellationToken)
    {
        var lista = conversas.ToList();
        if (lista.Count == 0)
            return new Dictionary<Guid, int>();

        var ids = lista.Select(c => c.ConversaId).Distinct().ToList();

        var naoLidasPorConversa = await _dbSet
            .AsNoTracking()
            .Where(m => ids.Contains(m.ConversaId) && m.AutorId != usuarioId)
            .Select(m => new { m.ConversaId, m.DataCriacao })
            .ToListAsync(cancellationToken);

        var leituraPorConversa = lista
            .GroupBy(c => c.ConversaId)
            .ToDictionary(g => g.Key, g => g.First().UltimaLeituraEm);

        return naoLidasPorConversa
            .Where(m => !leituraPorConversa.TryGetValue(m.ConversaId, out var leitura)
                        || !leitura.HasValue
                        || m.DataCriacao > leitura.Value)
            .GroupBy(m => m.ConversaId)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task AdicionarAsync(ChatMensagem mensagem, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(mensagem, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(ChatMensagem mensagem, CancellationToken cancellationToken)
    {
        _dbSet.Update(mensagem);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ChatMensagemReacao?> ObterReacaoAsync(Guid mensagemId, Guid usuarioId, string emoji, CancellationToken cancellationToken)
    {
        return await _context.Set<ChatMensagemReacao>()
            .FirstOrDefaultAsync(r => r.MensagemId == mensagemId && r.UsuarioId == usuarioId && r.Emoji == emoji, cancellationToken);
    }

    public async Task AdicionarReacaoAsync(ChatMensagemReacao reacao, CancellationToken cancellationToken)
    {
        await _context.Set<ChatMensagemReacao>().AddAsync(reacao, cancellationToken);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Toggle concorrente: a reação já foi inserida por outra requisição simultânea.
            // Tratamos como no-op para não retornar 500 — o estado final desejado já está persistido.
            _context.Entry(reacao).State = EntityState.Detached;
        }
    }

    public async Task RemoverReacaoAsync(ChatMensagemReacao reacao, CancellationToken cancellationToken)
    {
        _context.Set<ChatMensagemReacao>().Remove(reacao);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
