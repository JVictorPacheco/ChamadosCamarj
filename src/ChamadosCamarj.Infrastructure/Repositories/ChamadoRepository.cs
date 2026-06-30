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

    public async Task AdicionarComentarioAsync(Comentario comentario, CancellationToken cancellationToken = default)
    {
        if (comentario == null)
            throw new ArgumentNullException(nameof(comentario));

        await _context.Set<Comentario>().AddAsync(comentario, cancellationToken);
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

    public async Task<IEnumerable<Comentario>> ObterComentariosPorChamadoAsync(Guid chamadoId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Comentario>()
            .Where(c => c.ChamadoId == chamadoId)
            .OrderBy(c => c.DataCriacao)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
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

    public async Task<(IEnumerable<Chamado> Items, int Total)> ListarAsync(
        int pagina,
        int tamanhoPagina,
        Domain.Enums.StatusChamado? status = null,
        Domain.Enums.PrioridadeChamado? prioridade = null,
        Guid? responsavelId = null,
        Guid? categoriaId = null,
        string? busca = null,
        string? solicitanteEmail = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (prioridade.HasValue)
            query = query.Where(c => c.Prioridade == prioridade.Value);

        if (responsavelId.HasValue)
            query = query.Where(c => c.ResponsavelId == responsavelId.Value);

        if (categoriaId.HasValue)
            query = query.Where(c => c.CategoriaId == categoriaId.Value);

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(c => c.Titulo.Contains(busca) || c.Descricao.Contains(busca));

        if (!string.IsNullOrWhiteSpace(solicitanteEmail))
            query = query.Where(c => c.SolicitanteEmail == solicitanteEmail);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(c => c.Categoria)
            .Include(c => c.Comentarios)
            .Include(c => c.Anexos)
            .OrderByDescending(c => c.DataCriacao)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<int> ContarPorStatusAsync(Domain.Enums.StatusChamado status, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(c => c.Status == status, cancellationToken);
    }

    public async Task<int> ContarResolvidosHojeAsync(CancellationToken cancellationToken = default)
    {
        var hoje = DateTime.UtcNow.Date;
        return await _dbSet.CountAsync(c =>
            c.Status == Domain.Enums.StatusChamado.Resolvido &&
            c.DataConclusao.HasValue &&
            c.DataConclusao.Value.Date == hoje,
            cancellationToken);
    }

    public async Task<double?> ObterTempoMedioResolucaoHorasAsync(CancellationToken cancellationToken = default)
    {
        var resolvidos = await _dbSet
            .Where(c => c.Status == Domain.Enums.StatusChamado.Resolvido
                && c.DataConclusao.HasValue)
            .Select(c => new { c.DataCriacao, DataConclusao = c.DataConclusao!.Value })
            .ToListAsync(cancellationToken);

        if (resolvidos.Count == 0)
            return null;

        return resolvidos.Average(r => (r.DataConclusao - r.DataCriacao).TotalHours);
    }

    public async Task<Dictionary<string, int>> ContarPorCategoriaAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Status != Domain.Enums.StatusChamado.Fechado
                     && c.Status != Domain.Enums.StatusChamado.Cancelado)
            .GroupBy(c => c.Categoria != null ? c.Categoria.Nome : "Sem categoria")
            .Select(g => new { Categoria = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.Categoria, x => x.Quantidade, cancellationToken);
    }

    public async Task<Dictionary<string, int>> ContarPorPrioridadeAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Status != Domain.Enums.StatusChamado.Fechado
                     && c.Status != Domain.Enums.StatusChamado.Cancelado)
            .GroupBy(c => c.Prioridade)
            .Select(g => new { Prioridade = g.Key.ToString(), Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.Prioridade, x => x.Quantidade, cancellationToken);
    }

    public async Task<List<(DateTime Data, int Abertos, int Resolvidos)>> ObterTendenciaAsync(int dias, CancellationToken cancellationToken = default)
    {
        var inicio = DateTime.UtcNow.Date.AddDays(-dias + 1);
        var fim = DateTime.UtcNow.Date.AddDays(1);

        var chamadosNoPeriodo = await _dbSet
            .Where(c => c.DataCriacao >= inicio && c.DataCriacao < fim)
            .Select(c => new
            {
                Data = c.DataCriacao.Date,
                FoiResolvido = c.Status == Domain.Enums.StatusChamado.Resolvido && c.DataConclusao.HasValue
            })
            .ToListAsync(cancellationToken);

        return Enumerable.Range(0, dias)
            .Select(d => inicio.AddDays(d))
            .Select(data => (
                Data: data,
                Abertos: chamadosNoPeriodo.Count(c => c.Data == data),
                Resolvidos: chamadosNoPeriodo.Count(c => c.Data == data && c.FoiResolvido)
            ))
            .ToList();
    }
}
