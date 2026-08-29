using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Infrastructure.Services;

/// <summary>
/// Registrado quando o Supabase Storage não está configurado — permite a aplicação subir
/// normalmente, falhando de forma clara só se alguém tentar enviar arquivos no chat.
/// </summary>
public class NullChatStorageService : IChatStorageService
{
    private const string Mensagem = "Supabase Storage não está configurado (Supabase:Url/ServiceRoleKey ausentes). O envio de arquivos no chat está indisponível até essa configuração ser feita.";

    public Task<string> UploadAsync(string caminho, string contentType, Stream conteudo, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Mensagem);

    public Task<string> ObterUrlAssinadaAsync(string caminho, int expiracaoSegundos, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Mensagem);

    public Task RemoverAsync(string caminho, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Mensagem);
}
