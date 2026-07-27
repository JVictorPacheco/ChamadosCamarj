using System.Security.Cryptography;
using System.Text;

namespace ChamadosCamarj.Application.Common;

public static class ResetTokenHelper
{
    public static string GerarToken(string email, string signingKey, TimeSpan validade)
    {
        var expiry = DateTimeOffset.UtcNow.Add(validade).ToUnixTimeSeconds();
        var payload = $"{email}|{expiry}";
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var keyBytes = Encoding.UTF8.GetBytes(signingKey);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(payloadBytes);

        return $"{Convert.ToBase64String(payloadBytes)}.{Convert.ToBase64String(hash)}";
    }

    public static string? ValidarToken(string token, string signingKey)
    {
        var parts = token.Split('.');
        if (parts.Length != 2) return null;

        byte[] payloadBytes;
        byte[] hashBytes;
        try
        {
            payloadBytes = Convert.FromBase64String(parts[0]);
            hashBytes = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return null;
        }

        var keyBytes = Encoding.UTF8.GetBytes(signingKey);

        using var hmac = new HMACSHA256(keyBytes);
        var computedHash = hmac.ComputeHash(payloadBytes);

        if (!computedHash.SequenceEqual(hashBytes)) return null;

        var payload = Encoding.UTF8.GetString(payloadBytes);
        var parts2 = payload.Split('|');
        if (parts2.Length != 2) return null;

        if (!long.TryParse(parts2[1], out var expirySeconds)) return null;

        var expiry = DateTimeOffset.FromUnixTimeSeconds(expirySeconds);
        if (DateTimeOffset.UtcNow > expiry) return null;

        return parts2[0];
    }
}
