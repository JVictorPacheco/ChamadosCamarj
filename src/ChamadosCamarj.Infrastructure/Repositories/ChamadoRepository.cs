using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;
using ChamadosCamarj.Application.Common.Exceptions;

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

        try
        {
            _dbSet.Update(chamado);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("Este chamado foi modificado por outro usuario. Recarregue a pagina e tente novamente.");
        }
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
            .Include(c => c.Area)
            .Include(c => c.Tipo)
            .Include(c => c.Comentarios)
            .Include(c => c.Anexos)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Chamado?> ObterPorIdComTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Area)
            .Include(c => c.Tipo)
            .Include(c => c.Comentarios)
            .Include(c => c.Anexos)
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
            .Include(c => c.Area)
            .Include(c => c.Tipo)
            .Include(c => c.Comentarios)
            .Include(c => c.Anexos)
            .AsNoTracking()
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Chamado>> ObterPorStatusAsync(Domain.Enums.StatusChamado status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Area)
            .Include(c => c.Tipo)
            .AsNoTracking()
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Chamado>> ObterPorSolicitanteAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Area)
            .Include(c => c.Tipo)
            .AsNoTracking()
            .Where(c => c.SolicitanteEmail == email)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Chamado>> ObterPorResponsavelAsync(Guid responsavelId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Area)
            .Include(c => c.Tipo)
            .AsNoTracking()
            .Where(c => c.ResponsavelId == responsavelId)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Chamado>> ObterAtrasadosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Area)
            .Include(c => c.Tipo)
            .AsNoTracking()
            .Where(c => c.DataLimite != null && c.DataLimite < DateTime.UtcNow && c.Status != Domain.Enums.StatusChamado.Resolvido && c.Status != Domain.Enums.StatusChamado.Fechado && c.Status != Domain.Enums.StatusChamado.Cancelado)
            .OrderBy(c => c.DataLimite)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Regra ÚNICA de "quem pode ver qual chamado" — usada pela listagem, pelo PodeVerAsync e pelas
    /// métricas do Dashboard. Spec: .specs/features/autorizacao-chamados (AC-01, AC-02, AC-08, AC-11).
    /// "Chamado do grupo" = aberto por um membro do grupo OU com responsável membro do grupo OU
    /// com área = o grupo (spec area-e-tipo-do-chamado AC-07).
    /// </summary>
    private IQueryable<Chamado> AplicarVisibilidade(IQueryable<Chamado> query, ContextoAcesso acesso)
    {
        if (acesso.Perfil == Perfil.Admin)
            return query;

        var usuarioId = acesso.UsuarioId;
        var email = (acesso.Email ?? string.Empty).Trim().ToLowerInvariant();
        var atendente = acesso.Perfil == Perfil.Atendente;

        if (!acesso.GrupoId.HasValue)
        {
            return atendente
                ? query.Where(c => c.ResponsavelId == null
                                || c.ResponsavelId == usuarioId
                                || c.SolicitanteEmail.ToLower() == email)
                : query.Where(c => c.SolicitanteEmail.ToLower() == email);
        }

        var grupoId = acesso.GrupoId.Value;
        var membros = _context.UsuariosPerfil.Where(u => u.GrupoId == grupoId);

        return atendente
            ? query.Where(c => c.ResponsavelId == null
                            || c.ResponsavelId == usuarioId
                            || c.SolicitanteEmail.ToLower() == email
                            || c.AreaId == grupoId
                            || membros.Any(u => u.Id == c.ResponsavelId || u.Email == c.SolicitanteEmail.ToLower()))
            : query.Where(c => c.SolicitanteEmail.ToLower() == email
                            || c.AreaId == grupoId
                            || membros.Any(u => u.Id == c.ResponsavelId || u.Email == c.SolicitanteEmail.ToLower()));
    }

    public async Task<bool> PodeVerAsync(Guid chamadoId, ContextoAcesso acesso, CancellationToken cancellationToken = default)
    {
        return await AplicarVisibilidade(_dbSet.AsNoTracking().Where(c => c.Id == chamadoId), acesso)
            .AnyAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Chamado> Items, int Total)> ListarAsync(
        ContextoAcesso acesso,
        int pagina,
        int tamanhoPagina,
        Domain.Enums.StatusChamado? status = null,
        Domain.Enums.PrioridadeChamado? prioridade = null,
        Guid? responsavelId = null,
        Guid? areaId = null,
        Guid? tipoId = null,
        string? busca = null,
        string? solicitanteEmail = null,
        IEnumerable<Domain.Enums.StatusChamado>? statusEntre = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        Domain.Enums.MotivoEncerramento? motivoEncerramento = null,
        CancellationToken cancellationToken = default)
    {
        var query = AplicarVisibilidade(_dbSet.AsNoTracking(), acesso);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (prioridade.HasValue)
            query = query.Where(c => c.Prioridade == prioridade.Value);

        if (responsavelId.HasValue)
            query = query.Where(c => c.ResponsavelId == responsavelId.Value);

        if (areaId.HasValue)
            query = query.Where(c => c.AreaId == areaId.Value);

        if (tipoId.HasValue)
            query = query.Where(c => c.TipoId == tipoId.Value);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var numeroBuscado = ParseNumeroChamado(busca);
            query = numeroBuscado.HasValue
                ? query.Where(c => c.Titulo.Contains(busca) || c.Descricao.Contains(busca) || c.Numero == numeroBuscado.Value)
                : query.Where(c => c.Titulo.Contains(busca) || c.Descricao.Contains(busca));
        }

        // Filtro comum, aplicado POR CIMA da visibilidade — nunca a substitui.
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
            .Include(c => c.Area)
            .Include(c => c.Tipo)
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

    public async Task<Dictionary<Domain.Enums.StatusChamado, int>> ContarPorStatusAgrupadoAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default)
    {
        return await AplicarVisibilidade(_dbSet.AsNoTracking(), acesso)
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Quantidade, cancellationToken);
    }

    public async Task<(int TotalResolvidos, int DentroPrazo)> ContarSlaComplianceAsync(ContextoAcesso acesso, DateTime inicio, DateTime fim, CancellationToken cancellationToken = default)
    {
        var resolvidos = await AplicarVisibilidade(_dbSet.AsNoTracking(), acesso)
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

    public async Task<int> ContarResolvidosHojeAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default)
    {
        var hoje = DateTime.UtcNow.Date;
        return await AplicarVisibilidade(_dbSet.AsNoTracking(), acesso).CountAsync(c =>
            c.Status == Domain.Enums.StatusChamado.Resolvido &&
            c.DataConclusao.HasValue &&
            c.DataConclusao.Value.Date == hoje,
            cancellationToken);
    }

    public async Task<double?> ObterTempoMedioResolucaoHorasAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default)
    {
        var resolvidos = await AplicarVisibilidade(_dbSet.AsNoTracking(), acesso)
            .Where(c => c.Status == Domain.Enums.StatusChamado.Resolvido
                && c.DataConclusao.HasValue)
            .Select(c => new { c.DataCriacao, DataConclusao = c.DataConclusao!.Value })
            .ToListAsync(cancellationToken);

        if (resolvidos.Count == 0)
            return null;

        return resolvidos.Average(r => (r.DataConclusao - r.DataCriacao).TotalHours);
    }

    public async Task<List<ContagemPorNome>> ContarPorAreaAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default)
    {
        return await AplicarVisibilidade(_dbSet.AsNoTracking(), acesso)
            .Where(c => c.Status != Domain.Enums.StatusChamado.Fechado
                     && c.Status != Domain.Enums.StatusChamado.Cancelado)
            .GroupBy(c => new { c.AreaId, Nome = c.Area != null ? c.Area.Nome : "Sem área" })
            .Select(g => new ContagemPorNome(g.Key.Nome, g.Key.AreaId, g.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ContagemPorNome>> ContarPorTipoAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default)
    {
        return await AplicarVisibilidade(_dbSet.AsNoTracking(), acesso)
            .Where(c => c.Status != Domain.Enums.StatusChamado.Fechado
                     && c.Status != Domain.Enums.StatusChamado.Cancelado)
            .GroupBy(c => new { c.TipoId, Nome = c.Tipo != null ? c.Tipo.Nome : "Não classificado" })
            .Select(g => new ContagemPorNome(g.Key.Nome, g.Key.TipoId, g.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> ContarPorPrioridadeAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default)
    {
        return await AplicarVisibilidade(_dbSet.AsNoTracking(), acesso)
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
