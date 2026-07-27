namespace ChamadosCamarj.Domain.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(string caminho, string contentType, Stream conteudo, CancellationToken cancellationToken = default);
    Task<string> ObterUrlAssinadaAsync(string caminho, int expiracaoSegundos, CancellationToken cancellationToken = default);
    Task RemoverAsync(string caminho, CancellationToken cancellationToken = default);
}
