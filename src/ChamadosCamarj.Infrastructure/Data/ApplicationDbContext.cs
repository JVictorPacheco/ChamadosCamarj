using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Chamado> Chamados => Set<Chamado>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    // Legado (substituída por Área + Tipo); tabela mantida até a limpeza pós-deploy.
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<TipoChamado> TiposChamado => Set<TipoChamado>();
    public DbSet<Anexo> Anexos => Set<Anexo>();
    public DbSet<HistoricoEntrada> Historico => Set<HistoricoEntrada>();
    public DbSet<UsuarioPerfil> UsuariosPerfil => Set<UsuarioPerfil>();
    public DbSet<Grupo> Grupos => Set<Grupo>();
    public DbSet<ChatConversa> ChatConversas => Set<ChatConversa>();
    public DbSet<ChatParticipante> ChatParticipantes => Set<ChatParticipante>();
    public DbSet<ChatMensagem> ChatMensagens => Set<ChatMensagem>();
    public DbSet<ChatMensagemReacao> ChatMensagemReacoes => Set<ChatMensagemReacao>();
    public DbSet<ChatPresenca> ChatPresencas => Set<ChatPresenca>();
    public DbSet<ChatHistorico> ChatHistoricos => Set<ChatHistorico>();
    public DbSet<AuditoriaAcesso> AuditoriaAcessos => Set<AuditoriaAcesso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
