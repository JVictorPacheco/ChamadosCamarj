using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Infrastructure.Services;

public class SupabaseStorageService : IStorageService
{
    private readonly Supabase.Client _client;
    private readonly string _bucket;
    private readonly ILogger<SupabaseStorageService> _logger;

    public SupabaseStorageService(Supabase.Client client, IOptions<SupabaseSettings> settings, ILogger<SupabaseStorageService> logger)
    {
        _client = client;
        _bucket = settings.Value.Bucket;
        _logger = logger;
    }

    public async Task<string> UploadAsync(string caminho, string contentType, Stream conteudo, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await conteudo.CopyToAsync(memoryStream, cancellationToken);

        var bytes = memoryStream.ToArray();
        _logger.LogInformation("Iniciando upload para Supabase Storage: bucket={Bucket}, caminho={Caminho}, tamanho={Tamanho}",
            _bucket, caminho, bytes.Length);

        try
        {
            await _client.Storage
                .From(_bucket)
                .Upload(bytes, caminho, new Supabase.Storage.FileOptions { ContentType = contentType, Upsert = false },
                    cancellationToken: cancellationToken);

            _logger.LogInformation("Upload concluido com sucesso: caminho={Caminho}", caminho);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha no upload para Supabase Storage: bucket={Bucket}, caminho={Caminho}, contentType={ContentType}",
                _bucket, caminho, contentType);
            throw;
        }

        return caminho;
    }

    public async Task<string> ObterUrlAssinadaAsync(string caminho, int expiracaoSegundos, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Gerando URL assinada: caminho={Caminho}, expiracao={ExpiracaoSegundos}s", caminho, expiracaoSegundos);

        try
        {
            var url = await _client.Storage
                .From(_bucket)
                .CreateSignedUrl(caminho, expiracaoSegundos);

            var urlLimpa = url.TrimEnd('?');
            return urlLimpa;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao gerar URL assinada: bucket={Bucket}, caminho={Caminho}", _bucket, caminho);
            throw;
        }
    }

    public async Task RemoverAsync(string caminho, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removendo arquivo do Supabase Storage: bucket={Bucket}, caminho={Caminho}", _bucket, caminho);

        try
        {
            await _client.Storage
                .From(_bucket)
                .Remove(new List<string> { caminho });

            _logger.LogInformation("Arquivo removido com sucesso: caminho={Caminho}", caminho);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao remover arquivo do Supabase Storage: bucket={Bucket}, caminho={Caminho}", _bucket, caminho);
            throw;
        }
    }
}
