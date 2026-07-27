using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Application.Common;

public class JwtTokenService : IJwtTokenService
{
    private readonly AuthSettings _authSettings;

    public JwtTokenService(IOptions<AuthSettings> authSettings)
    {
        _authSettings = authSettings.Value;
    }

    public string GerarToken(UsuarioPerfil usuario)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authSettings.JwtSigningKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim("perfil", usuario.Perfil.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: "ChamadosCamarj",
            audience: "ChamadosCamarj.Frontend",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_authSettings.TokenExpiracaoHoras),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
