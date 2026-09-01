using ChamadosCamarj.Domain.Common;

namespace ChamadosCamarj.Domain.Entities;

public class ChatParticipante : BaseEntity
{
    private ChatParticipante() { }

    public ChatParticipante(Guid conversaId, Guid usuarioId, string usuarioNome)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("UsuarioId não pode ser vazio.", nameof(usuarioId));
        if (string.IsNullOrWhiteSpace(usuarioNome))
            throw new ArgumentException("Nome do usuário é obrigatório.", nameof(usuarioNome));

        ConversaId = conversaId;
        UsuarioId = usuarioId;
        UsuarioNome = usuarioNome;
        Ativo = true;
    }

    public Guid ConversaId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string UsuarioNome { get; private set; } = string.Empty;
    public DateTime? UltimaLeituraEm { get; private set; }
    public bool Ativo { get; private set; } = true;

    // Navegação EF
    public ChatConversa Conversa { get; private set; } = null!;

    public void MarcarComoLido()
    {
        UltimaLeituraEm = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo = false;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Reativar()
    {
        Ativo = true;
        DataAtualizacao = DateTime.UtcNow;
    }
}
