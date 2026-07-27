using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Application.Common;

public interface IJwtTokenService
{
    string GerarToken(UsuarioPerfil usuario);
}
