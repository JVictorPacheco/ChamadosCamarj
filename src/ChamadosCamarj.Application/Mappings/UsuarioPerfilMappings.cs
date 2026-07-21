using ChamadosCamarj.Application.Features.Usuarios.DTOs;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Application.Mappings;

public static class UsuarioPerfilMappings
{
    public static UsuarioPerfilResponse ToResponse(this UsuarioPerfil usuarioPerfil) =>
        new(
            usuarioPerfil.Id,
            usuarioPerfil.Email,
            usuarioPerfil.Nome,
            usuarioPerfil.Perfil,
            usuarioPerfil.Ativo
        );
}
