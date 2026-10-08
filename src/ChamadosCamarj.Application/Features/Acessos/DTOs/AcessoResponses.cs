using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Acessos.DTOs;

/// <summary>Linha da lista do Controle de acesso (spec controle-de-acesso AC-01).</summary>
public record AcessoUsuarioResumoResponse(
    Guid Id,
    string Nome,
    string Email,
    Perfil Perfil,
    bool Ativo,
    bool AcessoTotal,
    IReadOnlyList<string> Modulos,
    ChatPerfil ChatPerfil,
    bool TemAjuste
);

/// <summary>Um módulo no painel da pessoa: o padrão do perfil, o que ela tem e se dá para mexer (AC-02).</summary>
public record ModuloAcessoItem(string Modulo, bool Padrao, bool Efetivo, bool Ajustavel);

public record AuditoriaAcessoResponse(DateTime DataHora, string AlteradoPorNome, string Item, string Anterior, string Novo);

/// <summary>Painel de uma pessoa no Controle de acesso (AC-02, AC-09, AC-14).</summary>
public record AcessoUsuarioDetalheResponse(
    Guid Id,
    string Nome,
    string Email,
    Perfil Perfil,
    bool AcessoTotal,
    IReadOnlyList<ModuloAcessoItem> Modulos,
    ChatPerfil ChatPerfil,
    IReadOnlyList<AuditoriaAcessoResponse> Auditoria
);
