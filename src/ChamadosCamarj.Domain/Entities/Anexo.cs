using ChamadosCamarj.Domain.Common;

namespace ChamadosCamarj.Domain.Entities;

public class Anexo : BaseEntity
{
    private Anexo() { }

    public Anexo(
        Guid chamadoId,
        string nomeArquivo,
        string caminhoStorage,
        string tipoArquivo,
        long tamanhoBytes,
        Guid? enviadoPorId = null,
        string enviadoPorNome = "Sistema",
        Guid? comentarioId = null)
    {
        if (string.IsNullOrWhiteSpace(nomeArquivo))
            throw new ArgumentException("Nome do arquivo é obrigatório.", nameof(nomeArquivo));
        if (string.IsNullOrWhiteSpace(caminhoStorage))
            throw new ArgumentException("Caminho no storage é obrigatório.", nameof(caminhoStorage));

        ChamadoId = chamadoId;
        ComentarioId = comentarioId;
        NomeArquivo = nomeArquivo;
        CaminhoStorage = caminhoStorage;
        TipoArquivo = tipoArquivo;
        TamanhoBytes = tamanhoBytes;
        EnviadoPorId = enviadoPorId;
        EnviadoPorNome = enviadoPorNome;
    }

    public Guid ChamadoId { get; private set; }
    public Guid? ComentarioId { get; private set; }
    public string NomeArquivo { get; private set; } = string.Empty;
    public string CaminhoStorage { get; private set; } = string.Empty;
    public string TipoArquivo { get; private set; } = string.Empty;
    public long TamanhoBytes { get; private set; }
    public Guid? EnviadoPorId { get; private set; }
    public string EnviadoPorNome { get; private set; } = string.Empty;

    // Navegação EF
    public Chamado? Chamado { get; private set; }
    public Comentario? Comentario { get; private set; }
}
