namespace ChamadosCamarj.Domain.Enums;

/// <summary>
/// O que um usuário está tentando fazer com um chamado. Usado pela política de permissões
/// (ChamadoPermissoes) para decidir quem pode o quê — ver matriz em
/// docs/obsidian/00 Visão/Perfis e Permissões.md.
/// </summary>
public enum AcaoChamado
{
    Ver,
    ComentarPublico,
    ComentarInterno,
    Anexar,
    Assumir,
    Resolver,
    Encerrar,
    Reabrir,
    AlterarStatus,
    Cancelar,
    Editar,
    ReclassificarTipo,
    Reatribuir,
    AlterarPrioridade,
    ForcarEncerramento
}
