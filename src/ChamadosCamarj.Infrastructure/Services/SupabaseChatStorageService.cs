using Microsoft.Extensions.Logging;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Infrastructure.Services;

/// <summary>
/// Armazenamento de arquivos do chat no bucket dedicado "chat-arquivos".
/// Reusa o mesmo Supabase.Client singleton já configurado para os anexos.
/// </summary>
// TODO: PENDENTE — o bucket "chat-arquivos" ainda NÃO foi criado no Supabase Storage
// (item em aberto no .specs/features/chat-corporativo/tasks.md). Sem ele, o envio de
// arquivos no chat falha com erro de bucket inexistente. Criar o bucket antes de
// habilitar o anexo de arquivos no chat em produção.
public class SupabaseChatStorageService : IChatStorageService
{
    private const string Bucket = "chat-arquivos";

    private readonly Supabase.Client _client;
    private readonly ILogger<SupabaseChatStorageService> _logger;

    public SupabaseChatStorageService(Supabase.Client client, ILogger<SupabaseChatStorageService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string> UploadAsync(string caminho, string contentType, Stream conteudo, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await conteudo.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        _logger.LogInformation("Upload chat storage: bucket={Bucket}, caminho={Caminho}, tamanho={Tamanho}", Bucket, caminho, bytes.Length);

        try
        {
            await _client.Storage
                .From(Bucket)
                .Upload(bytes, caminho, new Supabase.Storage.FileOptions { ContentType = contentType, Upsert = false },
                    cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha no upload para o bucket {Bucket}: caminho={Caminho}", Bucket, caminho);
            throw;
        }

        return caminho;
    }

    public async Task<string> ObterUrlAssinadaAsync(string caminho, int expiracaoSegundos, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = await _client.Storage
                .From(Bucket)
                .CreateSignedUrl(caminho, expiracaoSegundos);

            return url.TrimEnd('?');
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao gerar URL assinada no bucket {Bucket}: caminho={Caminho}", Bucket, caminho);
            throw;
        }
    }

    public async Task RemoverAsync(string caminho, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.Storage
                .From(Bucket)
                .Remove(new List<string> { caminho });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao remover arquivo do bucket {Bucket}: caminho={Caminho}", Bucket, caminho);
            throw;
        }
    }
}
