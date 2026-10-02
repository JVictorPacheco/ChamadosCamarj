using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Auth.DTOs;

public record AutenticacaoResponse(
    string Token,
    Guid Id,
    string Nome,
    string Email,
    Perfil Perfil,
    ChatPerfil ChatPerfil,
    bool MostrarConfirmacaoLeitura,
    Guid? GrupoId = null,
    // Módulos que a pessoa usa (spec controle-de-acesso) — monta o menu da tela.
    IReadOnlyList<string>? Modulos = null
);
