using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Repositories.Chat;

public class ChatConversaRepository : IChatConversaRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<ChatConversa> _dbSet;

    public ChatConversaRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<ChatConversa>();
    }

    public async Task<ChatConversa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Include(c => c.Participantes)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ChatConversa>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.Participantes)
            .Where(c => c.Ativa && c.Participantes.Any(p => p.UsuarioId == usuarioId && p.Ativo))
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatConversa?> ObterPrivadaEntreUsuariosAsync(Guid usuarioAId, Guid usuarioBId, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Include(c => c.Participantes)
            .Where(c => c.Tipo == ChatConversaTipo.Privada
                     && c.Participantes.Any(p => p.UsuarioId == usuarioAId)
                     && c.Participantes.Any(p => p.UsuarioId == usuarioBId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ChatParticipante?> ObterParticipanteAsync(Guid conversaId, Guid usuarioId, CancellationToken cancellationToken)
    {
        return await _context.Set<ChatParticipante>()
            .FirstOrDefaultAsync(p => p.ConversaId == conversaId && p.UsuarioId == usuarioId, cancellationToken);
    }

    public async Task<IEnumerable<ChatConversa>> ListarConversasComUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Include(c => c.Participantes)
            .Where(c => c.Participantes.Any(p => p.UsuarioId == usuarioId && p.Ativo))
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(ChatConversa conversa, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(conversa, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(ChatConversa conversa, CancellationToken cancellationToken)
    {
        _dbSet.Update(conversa);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarParticipanteAsync(ChatParticipante participante, CancellationToken cancellationToken)
    {
        _context.Set<ChatParticipante>().Update(participante);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
