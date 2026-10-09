using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.WebApi.Services;

/// <summary>
/// Confere o token contra o cadastro a cada pedido (spec perfil-no-cadastro). O perfil e a equipe que
/// vêm no JWT são os do dia do login (vale 10 h): aqui o servidor recusa quem foi desativado ou apagado e
/// troca os claims "perfil" e "grupo_id" pelos do cadastro. Como todo o resto do servidor só lê esses
/// claims (CurrentUserService, ChamadosHub), uma rota nova já nasce protegida (AC-20).
/// </summary>
public class CadastroClaimsValidator
{
    public const string ClaimPerfil = "perfil";
    public const string ClaimGrupo = "grupo_id";

    private readonly IUsuarioPerfilRepository _usuarios;

    public CadastroClaimsValidator(IUsuarioPerfilRepository usuarios) => _usuarios = usuarios;

    /// <returns>
    /// <c>true</c> se o token segue valendo (com os claims já atualizados); <c>false</c> se a conta não existe
    /// ou está desativada. Falha ao consultar o banco propaga: nunca aceita um token sem conferir.
    /// </returns>
    public async Task<bool> ValidarEAtualizarAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var valorId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(valorId, out var usuarioId))
            return false;

        var identidade = await _usuarios.ObterIdentidadeAsync(usuarioId, cancellationToken);
        if (identidade is null || !identidade.Ativo)
            return false;

        foreach (var claims in principal.Identities)
        {
            Substituir(claims, ClaimPerfil, identidade.Perfil.ToString());
            Substituir(claims, ClaimGrupo, identidade.GrupoId?.ToString());
        }

        return true;
    }

    private static void Substituir(ClaimsIdentity identidade, string tipo, string? valor)
    {
        foreach (var antigo in identidade.FindAll(tipo).ToList())
            identidade.RemoveClaim(antigo);

        if (valor is not null)
            identidade.AddClaim(new Claim(tipo, valor));
    }
}
