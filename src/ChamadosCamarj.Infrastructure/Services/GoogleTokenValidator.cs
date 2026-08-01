using Google.Apis.Auth;
using ChamadosCamarj.Application.Common;

namespace ChamadosCamarj.Infrastructure.Services;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    public Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken, string audience) =>
        GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [audience],
        });
}
