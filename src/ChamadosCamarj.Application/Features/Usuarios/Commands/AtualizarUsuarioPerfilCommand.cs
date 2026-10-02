using MediatR;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Usuarios.Commands;

public record AtualizarUsuarioPerfilCommand(
    Guid Id,
    string Nome,
    Perfil Perfil,
    bool Ativo,
    // Opcional desde o controle de acesso (spec controle-de-acesso AC-10): a tela de Usuários não manda mais;
    // null = não mexe no Chat. Quem ainda mandar continua funcionando.
    ChatPerfil? ChatPerfil,
    Guid? GrupoId = null,
    string? PerfilRequisitante = null,
    Guid RequisitanteId = default,
    string RequisitanteNome = ""
) : IRequest<UsuarioPerfilResponse?>;
