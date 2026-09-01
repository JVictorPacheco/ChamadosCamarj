using MediatR;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Usuarios.Commands;

public record AtualizarUsuarioPerfilCommand(
    Guid Id,
    string Nome,
    Perfil Perfil,
    bool Ativo,
    ChatPerfil ChatPerfil,
    Guid? GrupoId = null,
    string? PerfilRequisitante = null,
    Guid RequisitanteId = default,
    string RequisitanteNome = ""
) : IRequest<UsuarioPerfilResponse?>;
