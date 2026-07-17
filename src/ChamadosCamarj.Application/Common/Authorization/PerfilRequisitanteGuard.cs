using ChamadosCamarj.Application.Common.Exceptions;

namespace ChamadosCamarj.Application.Common.Authorization;

/// <summary>
/// Guarda de autorização "só Admin" usada pelos Handlers de Usuarios.
/// Centraliza a checagem que antes vivia só no Controller — assim qualquer novo caller
/// que invoque os Commands/Queries diretamente (fora do Controller) também é protegido.
/// </summary>
public static class PerfilRequisitanteGuard
{
    private const string PerfilAdmin = "Admin";

    public static void ExigirAdmin(string? perfilRequisitante)
    {
        if (!string.Equals(perfilRequisitante, PerfilAdmin, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException("Apenas usuários com perfil Admin podem realizar esta ação.");
    }
}
