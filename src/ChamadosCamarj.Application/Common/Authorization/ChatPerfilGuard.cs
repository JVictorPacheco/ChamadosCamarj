using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Common.Authorization;

/// <summary>
/// Guarda de autorização por ChatPerfil. Como o ChatPerfil não vive no JWT, os handlers
/// carregam o UsuarioPerfil do banco e usam estas checagens.
/// </summary>
public static class ChatPerfilGuard
{
    public static void ExigirAcesso(ChatPerfil perfil)
    {
        if (perfil == ChatPerfil.SemAcesso)
            throw new ForbiddenException("Você não tem acesso ao chat.");
    }

    public static void ExigirCriadorDeGrupo(ChatPerfil perfil)
    {
        if (perfil != ChatPerfil.CriadorDeGrupo)
            throw new ForbiddenException("Você não tem permissão para criar grupos.");
    }
}
