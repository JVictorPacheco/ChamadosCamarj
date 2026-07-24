using Microsoft.Extensions.Options;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Infrastructure.Services;

public class SupabaseStorageService : IStorageService
{
    private readonly Supabase.Client _client;
    private readonly string _bucket;

    public SupabaseStorageService(Supabase.Client client, IOptions<SupabaseSettings> settings)
    {
        _client = client;
        _bucket = settings.Value.Bucket;
    }

    public async Task<string> UploadAsync(string caminho, string contentType, Stream conteudo, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await conteudo.CopyToAsync(memoryStream, cancellationToken);

        await _client.Storage
            .From(_bucket)
            .Upload(memoryStream.ToArray(), caminho, new Supabase.Storage.FileOptions { ContentType = contentType, Upsert = false });

        return caminho;
    }

    public async Task<string> ObterUrlAssinadaAsync(string caminho, int expiracaoSegundos, CancellationToken cancellationToken = default)
    {
        var url = await _client.Storage
            .From(_bucket)
            .CreateSignedUrl(caminho, expiracaoSegundos);

        // O SDK (Supabase v1.3.0) devolve a URL com um "?" solto no final quando nenhuma
        // TransformOptions/download é passada — isso quebra o parse do token JWT no servidor
        // do Storage ("Failed to base64url decode the signature"). Removido aqui até o SDK
        // corrigir isso upstream (não achado como issue já reportada em 2026-07-21).
        return url.TrimEnd('?');
    }

    public async Task RemoverAsync(string caminho, CancellationToken cancellationToken = default)
    {
        await _client.Storage
            .From(_bucket)
            .Remove(new List<string> { caminho });
    }
}
