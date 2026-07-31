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

    public async Task AdicionarAnexoAsync(Anexo anexo, CancellationToken cancellationToken = default)
    {
        if (anexo == null)
            throw new ArgumentNullException(nameof(anexo));

        await _context.Set<Anexo>().AddAsync(anexo, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Chamado?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Categoria)
            .Include(c => c.Comentarios)
            .Include(c => c.Anexos)
            .AsNoTracking()
            .AsSplitQuery()
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

    public async Task RemoverAnexoAsync(Guid anexoId, CancellationToken cancellationToken = default)
    {
        var anexo = await _context.Set<Anexo>().FirstOrDefaultAsync(a => a.Id == anexoId, cancellationToken);
        if (anexo is null) return;

        _context.Set<Anexo>().Remove(anexo);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Anexo>> ObterAnexosPorChamadoAsync(Guid chamadoId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Anexo>()
            .Where(a => a.ChamadoId == chamadoId)
            .OrderBy(a => a.DataCriacao)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Anexo?> ObterAnexoPorIdAsync(Guid anexoId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Anexo>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == anexoId, cancellationToken);
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
        IEnumerable<Domain.Enums.StatusChamado>? statusEntre = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        Guid? usuarioLogadoId = null,
        Guid? grupoId = null,
        Domain.Enums.MotivoEncerramento? motivoEncerramento = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (grupoId.HasValue && usuarioLogadoId.HasValue)
        {
            query = query.Where(c => c.ResponsavelId.HasValue &&
                (c.ResponsavelId == usuarioLogadoId.Value ||
                 _context.UsuariosPerfil.Any(u => u.Id == c.ResponsavelId && u.GrupoId == grupoId.Value)));
        }

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (prioridade.HasValue)
            query = query.Where(c => c.Prioridade == prioridade.Value);

        if (responsavelId.HasValue)
            query = query.Where(c => c.ResponsavelId == responsavelId.Value);

        if (categoriaId.HasValue)
            query = query.Where(c => c.CategoriaId == categoriaId.Value);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var numeroBuscado = ParseNumeroChamado(busca);
            query = numeroBuscado.HasValue
                ? query.Where(c => c.Titulo.Contains(busca) || c.Descricao.Contains(busca) || c.Numero == numeroBuscado.Value)
                : query.Where(c => c.Titulo.Contains(busca) || c.Descricao.Contains(busca));
        }

        if (!string.IsNullOrWhiteSpace(solicitanteEmail))
            query = query.Where(c => c.SolicitanteEmail == solicitanteEmail);

        if (statusEntre is not null)
            query = query.Where(c => statusEntre.Contains(c.Status));

        if (dataInicio.HasValue)
            query = query.Where(c => c.DataCriacao >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(c => c.DataCriacao <= dataFim.Value);

        if (motivoEncerramento.HasValue)
            query = query.Where(c => c.MotivoEncerramento == motivoEncerramento.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(c => c.Categoria)
            .Include(c => c.Comentarios)
            .Include(c => c.Anexos)
            .AsSplitQuery()
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

    public async Task<Dictionary<Domain.Enums.StatusChamado, int>> ContarPorStatusAgrupadoAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Quantidade, cancellationToken);
    }

    public async Task<(int TotalResolvidos, int DentroPrazo)> ContarSlaComplianceAsync(DateTime inicio, DateTime fim, CancellationToken cancellationToken = default)
    {
        var resolvidos = await _dbSet
            .AsNoTracking()
            .Where(c => c.DataConclusao.HasValue
                && c.DataConclusao >= inicio
                && c.DataConclusao <= fim
                && (c.Status == Domain.Enums.StatusChamado.Resolvido || c.Status == Domain.Enums.StatusChamado.Fechado))
            .Select(c => new { c.DataConclusao, c.DataLimite })
            .ToListAsync(cancellationToken);

        var total = resolvidos.Count;
        var dentroPrazo = resolvidos.Count(r => r.DataLimite.HasValue && r.DataConclusao <= r.DataLimite);
        return (total, dentroPrazo);
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

    // Aceita "42" ou "CAM-42" (case-insensitive) na mesma busca de texto livre —
    // qualquer outra coisa (ex: "impressora") não é um número, cai só na busca por texto.
    private static int? ParseNumeroChamado(string busca)
    {
        var texto = busca.Trim();
        if (texto.StartsWith("CAM-", StringComparison.OrdinalIgnoreCase))
            texto = texto[4..];

        return int.TryParse(texto, out var numero) ? numero : null;
    }
}
