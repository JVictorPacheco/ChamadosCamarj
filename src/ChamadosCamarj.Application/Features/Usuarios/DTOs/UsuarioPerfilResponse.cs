using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Usuarios.DTOs;

public record UsuarioPerfilResponse(
    Guid Id,
    string Email,
    string Nome,
    Perfil Perfil,
    bool Ativo,
    Guid? GrupoId = null,
    string? GrupoNome = null
);
