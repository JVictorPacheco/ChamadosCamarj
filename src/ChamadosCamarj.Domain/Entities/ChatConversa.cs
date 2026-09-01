using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Domain.Entities;

public class ChatConversa : BaseEntity
{
    private ChatConversa() { }

    public ChatConversaTipo Tipo { get; private set; }
    public string? Nome { get; private set; }
    public Guid CriadoPorId { get; private set; }
    public bool Ativa { get; private set; } = true;

    // Navegação EF
    public ICollection<ChatParticipante> Participantes { get; private set; } = [];
    public ICollection<ChatMensagem> Mensagens { get; private set; } = [];

    public static ChatConversa CriarPrivada(Guid criadoPorId) =>
        new() { Tipo = ChatConversaTipo.Privada, CriadoPorId = criadoPorId };

    public static ChatConversa CriarGrupo(string nome, Guid criadoPorId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do grupo é obrigatório.", nameof(nome));

        return new ChatConversa { Tipo = ChatConversaTipo.Grupo, Nome = nome, CriadoPorId = criadoPorId };
    }

    public void AdicionarParticipante(ChatParticipante participante)
    {
        Participantes.Add(participante);
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativa = false;
        DataAtualizacao = DateTime.UtcNow;
    }
}
