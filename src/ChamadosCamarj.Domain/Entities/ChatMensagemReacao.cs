using ChamadosCamarj.Domain.Common;

namespace ChamadosCamarj.Domain.Entities;

public class ChatMensagemReacao : BaseEntity
{
    private ChatMensagemReacao() { }

    public ChatMensagemReacao(Guid mensagemId, Guid usuarioId, string usuarioNome, string emoji)
    {
        if (string.IsNullOrWhiteSpace(emoji))
            throw new ArgumentException("Emoji é obrigatório.", nameof(emoji));

        MensagemId = mensagemId;
        UsuarioId = usuarioId;
        UsuarioNome = usuarioNome;
        Emoji = emoji;
    }

    public Guid MensagemId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string UsuarioNome { get; private set; } = string.Empty;
    public string Emoji { get; private set; } = string.Empty;

    // Navegação EF
    public ChatMensagem Mensagem { get; private set; } = null!;
}
