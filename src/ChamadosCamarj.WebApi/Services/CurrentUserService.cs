using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ChamadosCamarj.Application.Common;

namespace ChamadosCamarj.WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid UsuarioId
    {
        get
        {
            var valor = User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(valor, out var id) ? id : Guid.Empty;
        }
    }

    public string Nome => User?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    public string Perfil => User?.FindFirstValue("perfil") ?? string.Empty;

    public Guid? GrupoId
    {
        get
        {
            var valor = User?.FindFirstValue("grupo_id");
            return Guid.TryParse(valor, out var id) ? id : null;
        }
    }
}
