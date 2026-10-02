using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Common.Autorizacao;

/// <summary>
/// O que a regra de permissão precisa saber do chamado (além de quem está pedindo).
/// </summary>
public sealed record DadosDoChamado(string SolicitanteEmail, Guid? ResponsavelId, StatusChamado Status);

/// <summary>
/// Quem pode fazer o quê num chamado que já se sabe que o usuário consegue VER
/// (a visibilidade é decidida antes, pelo repositório). Espelha a matriz de
/// docs/obsidian/00 Visão/Perfis e Permissões.md — spec: .specs/features/autorizacao-chamados
/// e, para Editar, .specs/features/editar-chamado.
/// </summary>
public static class ChamadoPermissoes
{
    public const string MensagemEdicaoEncerrado =
        "Não é possível editar um chamado encerrado. Reabra o chamado para editá-lo.";

    public static bool Pode(AcaoChamado acao, ContextoAcesso acesso, DadosDoChamado chamado)
    {
        // Editar tem regra própria, que vale inclusive para o Admin (encerrado ninguém edita).
        if (acao == AcaoChamado.Editar)
            return PodeEditar(acesso, chamado);

        if (acesso.Perfil == Perfil.Admin)
            return true;

        var atendente = acesso.Perfil == Perfil.Atendente;

        return acao switch
        {
            AcaoChamado.Ver or AcaoChamado.ComentarPublico or AcaoChamado.Anexar => true,

            AcaoChamado.Cancelar => atendente || AbriuOChamado(acesso, chamado.SolicitanteEmail),

            AcaoChamado.ComentarInterno or AcaoChamado.Assumir or AcaoChamado.Resolver
                or AcaoChamado.Encerrar or AcaoChamado.Reabrir or AcaoChamado.AlterarStatus
                or AcaoChamado.ReclassificarTipo => atendente,

            // Reatribuir, AlterarPrioridade, ForcarEncerramento: só Admin (já tratado acima)
            _ => false
        };
    }

    /// <summary>
    /// spec editar-chamado: não encerrado E (Admin OU responsável atual OU quem abriu enquanto ninguém
    /// assumiu). Vale para qualquer perfil — um Atendente que abriu o chamado edita como quem abriu.
    /// </summary>
    private static bool PodeEditar(ContextoAcesso acesso, DadosDoChamado chamado)
    {
        if (EdicaoBloqueadaPorEncerramento(chamado))
            return false;

        return acesso.Perfil == Perfil.Admin
            || (chamado.ResponsavelId.HasValue && chamado.ResponsavelId == acesso.UsuarioId)
            || (chamado.ResponsavelId is null && AbriuOChamado(acesso, chamado.SolicitanteEmail));
    }

    /// <summary>Resolvido, Fechado ou Cancelado não se edita — nem pelo Admin (spec editar-chamado AC-10).</summary>
    public static bool EdicaoBloqueadaPorEncerramento(DadosDoChamado chamado) =>
        chamado.Status is StatusChamado.Resolvido or StatusChamado.Fechado or StatusChamado.Cancelado;

    /// <summary>
    /// Ações cuja decisão depende de dados do chamado (quem abriu, responsável, status). Para as demais,
    /// o behaviour nem consulta o chamado.
    /// </summary>
    public static bool DependeDoChamado(AcaoChamado acao, Perfil perfil) => acao switch
    {
        AcaoChamado.Editar => true,
        AcaoChamado.Cancelar => perfil == Perfil.Solicitante,
        _ => false
    };

    /// <summary>
    /// Ações que alteram o chamado e por isso conferem a versão lida (spec correcoes-pre-deploy
    /// AC-09). Comentar e anexar só acrescentam e nunca dão conflito (AC-13).
    /// </summary>
    public static bool AlteraChamado(AcaoChamado acao) => acao is
        AcaoChamado.Assumir or AcaoChamado.Resolver or AcaoChamado.Encerrar or AcaoChamado.Reabrir
        or AcaoChamado.AlterarStatus or AcaoChamado.Cancelar or AcaoChamado.Editar
        or AcaoChamado.ReclassificarTipo or AcaoChamado.Reatribuir or AcaoChamado.AlterarPrioridade
        or AcaoChamado.ForcarEncerramento;

    private static bool AbriuOChamado(ContextoAcesso acesso, string solicitanteEmailDoChamado) =>
        !string.IsNullOrWhiteSpace(acesso.Email)
        && string.Equals(acesso.Email, solicitanteEmailDoChamado, StringComparison.OrdinalIgnoreCase);
}
