using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Common;

public static class CurrentUserServiceExtensions
{
    /// <summary>
    /// Monta o contexto de acesso a partir do usuário logado. Perfil ausente ou desconhecido no
    /// token vira Solicitante — o perfil mais restrito, que só enxerga os próprios chamados.
    /// </summary>
    public static ContextoAcesso ObterContextoAcesso(this ICurrentUserService currentUser)
    {
        var perfil = Enum.TryParse<Perfil>(currentUser.Perfil, ignoreCase: true, out var p) && Enum.IsDefined(p)
            ? p
            : Perfil.Solicitante;

        return new ContextoAcesso(currentUser.UsuarioId, currentUser.Email, perfil, currentUser.GrupoId);
    }
}
