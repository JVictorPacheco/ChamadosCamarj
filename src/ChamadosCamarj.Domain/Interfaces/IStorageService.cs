using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Domain.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(string nomeArquivo, string contentType, Stream conteudo, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadAsync(string caminho, CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(string caminho, CancellationToken cancellationToken = default);
}
