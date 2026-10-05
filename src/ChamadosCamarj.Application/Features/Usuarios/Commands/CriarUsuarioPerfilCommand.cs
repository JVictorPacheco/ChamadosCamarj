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
    ChatPerfil ChatPerfil = ChatPerfil.SemAcesso,
    string? PerfilRequisitante = null,
    // Quem está criando/reativando — para a auditoria de acessos (spec controle-de-acesso, review-2 R-04).
    Guid RequisitanteId = default,
    string RequisitanteNome = ""
) : IRequest<UsuarioPerfilResponse>;
