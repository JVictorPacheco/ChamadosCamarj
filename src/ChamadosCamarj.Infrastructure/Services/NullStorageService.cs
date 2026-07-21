using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Infrastructure.Services;

/// <summary>
/// Registrado quando o Supabase Storage não está configurado (Supabase:Url/ServiceRoleKey
/// ausentes em user-secrets) — permite a aplicação subir normalmente (mesma tolerância já
/// aplicada ao Auth:GoogleClientId), falhando de forma clara só se alguém tentar de fato
/// usar a feature de Anexos.
/// </summary>
public class NullStorageService : IStorageService
{
    private const string Mensagem = "Supabase Storage não está configurado (Supabase:Url/ServiceRoleKey ausentes). A feature de Anexos está indisponível até essa configuração ser feita.";

    public Task<string> UploadAsync(string caminho, string contentType, Stream conteudo, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Mensagem);

    public Task<string> ObterUrlAssinadaAsync(string caminho, int expiracaoSegundos, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Mensagem);
}
