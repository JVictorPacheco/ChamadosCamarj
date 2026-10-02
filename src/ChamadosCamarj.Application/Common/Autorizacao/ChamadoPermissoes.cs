using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Common.Autorizacao;

/// <summary>
/// Quem pode fazer o quê num chamado que já se sabe que o usuário consegue VER
/// (a visibilidade é decidida antes, pelo repositório). Espelha a matriz de
/// docs/obsidian/00 Visão/Perfis e Permissões.md — spec: .specs/features/autorizacao-chamados.
/// </summary>
public static class ChamadoPermissoes
{
    public static bool Pode(AcaoChamado acao, ContextoAcesso acesso, string solicitanteEmailDoChamado)
    {
        if (acesso.Perfil == Perfil.Admin)
            return true;

        var atendente = acesso.Perfil == Perfil.Atendente;

        return acao switch
        {
            AcaoChamado.Ver or AcaoChamado.ComentarPublico or AcaoChamado.Anexar => true,

            AcaoChamado.Cancelar => atendente || AbriuOChamado(acesso, solicitanteEmailDoChamado),

            AcaoChamado.ComentarInterno or AcaoChamado.Assumir or AcaoChamado.Resolver
                or AcaoChamado.Encerrar or AcaoChamado.Reabrir or AcaoChamado.AlterarStatus
                or AcaoChamado.Editar or AcaoChamado.ReclassificarTipo => atendente,

            // Reatribuir, AlterarPrioridade, ForcarEncerramento: só Admin (já tratado acima)
            _ => false
        };
    }

    /// <summary>
    /// Ações que alteram o chamado e por isso conferem a versão lida (spec correcoes-pre-deploy
    /// AC-09). Comentar e anexar só acrescentam e nunca dão conflito (AC-13).
    /// </summary>
    public static bool AlteraChamado(AcaoChamado acao) => acao is
        AcaoChamado.Assumir or AcaoChamado.Resolver or AcaoChamado.Encerrar or AcaoChamado.Reabrir
        or AcaoChamado.AlterarStatus or AcaoChamado.Cancelar or AcaoChamado.Editar
        or AcaoChamado.ReclassificarTipo or AcaoChamado.Reatribuir or AcaoChamado.AlterarPrioridade
        or AcaoChamado.ForcarEncerramento;

    /// <summary>Ações cuja decisão depende de quem abriu o chamado.</summary>
    public static bool DependeDoSolicitante(AcaoChamado acao) => acao == AcaoChamado.Cancelar;

    private static bool AbriuOChamado(ContextoAcesso acesso, string solicitanteEmailDoChamado) =>
        !string.IsNullOrWhiteSpace(acesso.Email)
        && string.Equals(acesso.Email, solicitanteEmailDoChamado, StringComparison.OrdinalIgnoreCase);
}
