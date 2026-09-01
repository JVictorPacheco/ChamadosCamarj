namespace ChamadosCamarj.Domain.Interfaces;

/// <summary>
/// Serviço de armazenamento de arquivos do chat corporativo.
/// Separado de <see cref="IStorageService"/> por usar um bucket dedicado (chat-arquivos).
/// </summary>
public interface IChatStorageService
{
    Task<string> UploadAsync(string caminho, string contentType, Stream conteudo, CancellationToken cancellationToken = default);
    Task<string> ObterUrlAssinadaAsync(string caminho, int expiracaoSegundos, CancellationToken cancellationToken = default);
    Task RemoverAsync(string caminho, CancellationToken cancellationToken = default);
}
