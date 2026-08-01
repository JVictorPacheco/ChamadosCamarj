using MediatR;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Usuarios.Commands;

public record CriarUsuarioPerfilCommand(
    string Email,
    string Nome,
    Perfil Perfil,
    string Senha,
    Guid? GrupoId = null,
    string? PerfilRequisitante = null
) : IRequest<UsuarioPerfilResponse>;
