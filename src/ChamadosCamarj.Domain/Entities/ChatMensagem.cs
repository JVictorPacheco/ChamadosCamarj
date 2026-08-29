using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Domain.Entities;

public class ChatMensagem : BaseEntity
{
    private ChatMensagem() { }

    public Guid ConversaId { get; private set; }
    public Guid AutorId { get; private set; }
    public string AutorNome { get; private set; } = string.Empty;
    public string? Conteudo { get; private set; }
    public string? ConteudoOriginal { get; private set; }
    public ChatMensagemTipo Tipo { get; private set; }
    public bool Deletada { get; private set; }
    public DateTime? EditadaEm { get; private set; }
    public Guid? RespostaParaMensagemId { get; private set; }

    // Arquivo (quando Tipo = Arquivo)
    public string? NomeArquivo { get; private set; }
    public string? CaminhoStorage { get; private set; }
    public string? TipoArquivo { get; private set; }
    public long? TamanhoBytes { get; private set; }

    // Navegação EF
    public ChatConversa Conversa { get; private set; } = null!;
    public ICollection<ChatMensagemReacao> Reacoes { get; private set; } = [];

    public static ChatMensagem CriarTexto(Guid conversaId, Guid autorId, string autorNome, string conteudo, Guid? respostaParaMensagemId = null)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new ArgumentException("Conteúdo da mensagem é obrigatório.", nameof(conteudo));

        return new ChatMensagem
        {
            ConversaId = conversaId,
            AutorId = autorId,
            AutorNome = autorNome,
            Conteudo = conteudo,
            Tipo = ChatMensagemTipo.Texto,
            RespostaParaMensagemId = respostaParaMensagemId
        };
    }

    public static ChatMensagem CriarArquivo(
        Guid conversaId, Guid autorId, string autorNome,
        string nomeArquivo, string caminhoStorage, string tipoArquivo, long tamanhoBytes,
        Guid? respostaParaMensagemId = null)
    {
        return new ChatMensagem
        {
            ConversaId = conversaId,
            AutorId = autorId,
            AutorNome = autorNome,
            Tipo = ChatMensagemTipo.Arquivo,
            NomeArquivo = nomeArquivo,
            CaminhoStorage = caminhoStorage,
            TipoArquivo = tipoArquivo,
            TamanhoBytes = tamanhoBytes,
            RespostaParaMensagemId = respostaParaMensagemId
        };
    }

    public static ChatMensagem CriarSistema(Guid conversaId, string conteudo)
    {
        return new ChatMensagem
        {
            ConversaId = conversaId,
            AutorId = Guid.Empty,
            AutorNome = "Sistema",
            Conteudo = conteudo,
            Tipo = ChatMensagemTipo.Sistema
        };
    }

    public void Editar(string novoConteudo)
    {
        if (string.IsNullOrWhiteSpace(novoConteudo))
            throw new ArgumentException("Conteúdo da mensagem é obrigatório.", nameof(novoConteudo));

        ConteudoOriginal ??= Conteudo;
        Conteudo = novoConteudo;
        EditadaEm = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Deletar()
    {
        ConteudoOriginal ??= Conteudo;
        Conteudo = null;
        Deletada = true;
        DataAtualizacao = DateTime.UtcNow;
    }
}
