using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace ChamadosCamarj.WebApi.Hubs;

/// <summary>
/// SignalR usa por padrão o claim NameIdentifier para identificar o usuário em
/// Clients.User(...). Como o JWT deste projeto usa o claim "sub" (JwtRegisteredClaimNames.Sub)
/// e MapInboundClaims está desativado, este provider lê o "sub" para casar com o UsuarioId.
/// </summary>
public class SubClaimUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
