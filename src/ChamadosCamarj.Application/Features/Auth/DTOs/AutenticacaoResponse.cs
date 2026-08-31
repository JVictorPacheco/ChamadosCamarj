using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Auth.DTOs;

public record AutenticacaoResponse(
    string Token,
    Guid Id,
    string Nome,
    string Email,
    Perfil Perfil,
    ChatPerfil ChatPerfil
);
