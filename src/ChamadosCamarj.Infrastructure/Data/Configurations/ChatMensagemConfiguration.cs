using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class ChatMensagemConfiguration : IEntityTypeConfiguration<ChatMensagem>
{
    public void Configure(EntityTypeBuilder<ChatMensagem> builder)
    {
        builder.ToTable("ChatMensagens");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.ConversaId)
            .IsRequired();

        builder.Property(m => m.AutorId)
            .IsRequired();

        builder.Property(m => m.AutorNome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(m => m.Conteudo)
            .IsRequired(false)
            .HasColumnType("text");

        builder.Property(m => m.ConteudoOriginal)
            .IsRequired(false)
            .HasColumnType("text");

        builder.Property(m => m.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.Deletada)
            .IsRequired();

        builder.Property(m => m.EditadaEm)
            .IsRequired(false);

        builder.Property(m => m.RespostaParaMensagemId)
            .IsRequired(false);

        builder.Property(m => m.NomeArquivo)
            .IsRequired(false)
            .HasMaxLength(300);

        builder.Property(m => m.CaminhoStorage)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(m => m.TipoArquivo)
            .IsRequired(false)
            .HasMaxLength(150);

        builder.Property(m => m.TamanhoBytes)
            .IsRequired(false);

        builder.HasMany(m => m.Reacoes)
            .WithOne(r => r.Mensagem)
            .HasForeignKey(r => r.MensagemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.ConversaId, m.DataCriacao });
    }
}
