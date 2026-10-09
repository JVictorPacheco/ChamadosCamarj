using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Repositories;

public class UsuarioPerfilRepository : IUsuarioPerfilRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<UsuarioPerfil> _dbSet;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Quanto a identidade (perfil, equipe, conta ativa) fica guardada (spec perfil-no-cadastro D4). Mudança feita
    /// pelo sistema invalida na hora; o prazo só limita edição direta no banco ou um 2º servidor.
    /// </summary>
    public static readonly TimeSpan ValidadeDaIdentidade = TimeSpan.FromSeconds(15);

    private static string ChaveIdentidade(Guid id) => $"identidade-usuario:{id}";

    public UsuarioPerfilRepository(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<UsuarioPerfil>();
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<UsuarioPerfil?> ObterPorEmailAsync(string email, CancellationToken ct)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<UsuarioPerfil?> ObterPorIdAsync(Guid id, CancellationToken ct)
    {
        return await _dbSet.AsNoTracking().Include(u => u.Grupo).FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<IdentidadeUsuario?> ObterIdentidadeAsync(Guid id, CancellationToken ct)
    {
        if (_cache.TryGetValue(ChaveIdentidade(id), out IdentidadeUsuario? guardada))
            return guardada;

        // Review R-01: anota a "geração" do usuário ANTES de ler o banco. Se uma gravação invalidar no meio da
        // leitura, a geração muda e o valor lido (já velho) não vai para o cache.
        var geracao = Geracao(id);

        var identidade = await LerIdentidadeAsync(id, ct);

        // Conta inexistente não é guardada: recusar de novo custa uma consulta e evita que uma conta recém-criada
        // fique invisível durante o prazo.
        if (identidade is not null && Geracao(id) == geracao)
            _cache.Set(ChaveIdentidade(id), identidade, ValidadeDaIdentidade);

        return identidade;
    }

    /// <summary>A leitura no banco, separada para o teste da corrida de cache (review R-01) poder intercalar uma gravação.</summary>
    protected virtual async Task<IdentidadeUsuario?> LerIdentidadeAsync(Guid id, CancellationToken ct) =>
        await _dbSet.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new IdentidadeUsuario(u.Perfil, u.Ativo, u.GrupoId))
            .FirstOrDefaultAsync(ct);

    // Uma entrada por usuário que já teve o cadastro gravado nesta execução: poucas dezenas de números.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, long> _geracoes = new();

    private static long Geracao(Guid id) => _geracoes.GetValueOrDefault(id);

    private void InvalidarIdentidade(Guid id)
    {
        _geracoes.AddOrUpdate(id, 1, (_, atual) => atual + 1);
        _cache.Remove(ChaveIdentidade(id));
    }

    public async Task<IEnumerable<UsuarioPerfil>> ListarAsync(CancellationToken ct)
    {
        return await _dbSet.AsNoTracking().Include(u => u.Grupo).OrderBy(u => u.Nome).ToListAsync(ct);
    }

    public async Task<IEnumerable<UsuarioPerfil>> ListarPorIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        return await _dbSet.AsNoTracking().Where(u => ids.Contains(u.Id)).ToListAsync(ct);
    }

    public async Task AdicionarAsync(UsuarioPerfil usuario, CancellationToken ct)
    {
        await _dbSet.AddAsync(usuario, ct);
        await _context.SaveChangesAsync(ct);
        InvalidarIdentidade(usuario.Id); // perfil-no-cadastro D4: a próxima conferência lê o cadastro novo
        _context.Entry(usuario).State = EntityState.Detached;
    }

    public async Task AtualizarAsync(UsuarioPerfil usuario, CancellationToken ct)
    {
        // Só o usuário, sem o grafo: Update() também passava a rastrear (e regravar) o Grupo carregado pelo
        // Include, que ficava preso depois do Detach abaixo e colidia na 2ª gravação da mesma requisição
        // (salvar módulos + Chat de quem tem Área → 500; review-3 R-01 de controle-de-acesso).
        _context.Entry(usuario).State = EntityState.Modified;
        await _context.SaveChangesAsync(ct);
        InvalidarIdentidade(usuario.Id); // perfil-no-cadastro D4: a próxima conferência lê o cadastro novo
        // Achado ao vivo pós-revisão (não estava no relatório): AtualizarUsuarioPerfilCommandHandler
        // salva o mesmo usuário duas vezes por requisição — uma vez pros campos gerais, outra
        // (indiretamente, via DefinirChatPerfilCommand) pro ChatPerfil — cada Handle carrega sua
        // própria cópia via ObterPorIdAsync. Sem detach aqui, o segundo Update(), com uma instância
        // C# diferente mas a mesma PK, colide no change tracker do EF Core (mesmo DbContext,
        // compartilhado dentro da requisição via escopo do MediatR) e derruba a requisição com 500 —
        // reproduzido ao vivo revogando acesso pelo dialog "Editar usuário", não pego por nenhum
        // teste (todos usam Moq, que não modela o change tracker real).
        _context.Entry(usuario).State = EntityState.Detached;
    }
}
