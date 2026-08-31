using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Chamado> Chamados => Set<Chamado>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
