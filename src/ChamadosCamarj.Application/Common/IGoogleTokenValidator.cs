using Google.Apis.Auth;

namespace ChamadosCamarj.Application.Common;

/// <summary>
/// Abstrai a validação do id_token do Google (Google.Apis.Auth.GoogleJsonWebSignature é estático,
/// não mockável diretamente) para permitir testar o AutenticarGoogleCommandHandler sem depender
/// de rede/serviço do Google.
/// </summary>
public interface IGoogleTokenValidator
{
    Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken, string audience);
}
