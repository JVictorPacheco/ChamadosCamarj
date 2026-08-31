using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Domain.Entities;

public class ChatHistorico : BaseEntity
{
    private ChatHistorico() { }

    public Guid UsuarioId { get; private set; }
    public string UsuarioNome { get; private set; } = string.Empty;
    public ChatAcao Acao { get; private set; }
    public string? Detalhe { get; private set; }
    public Guid? ConversaId { get; private set; }
    public Guid? MensagemId { get; private set; }

    public static ChatHistorico Criar(
        Guid usuarioId,
        string usuarioNome,
        ChatAcao acao,
        string? detalhe = null,
        Guid? conversaId = null,
        Guid? mensagemId = null)
        => new()
        {
            UsuarioId = usuarioId,
            UsuarioNome = usuarioNome,
            Acao = acao,
            Detalhe = detalhe,
            ConversaId = conversaId,
            MensagemId = mensagemId
        };
}
