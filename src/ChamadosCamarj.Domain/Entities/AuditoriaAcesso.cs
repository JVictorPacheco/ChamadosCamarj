using ChamadosCamarj.Domain.Common;

namespace ChamadosCamarj.Domain.Entities;

/// <summary>
/// Registro de uma mudança de acesso de uma pessoa (spec controle-de-acesso AC-14): um módulo, o nível
/// do Chat ou o perfil, com o antes e o depois.
/// </summary>
public class AuditoriaAcesso : BaseEntity
{
    private AuditoriaAcesso() { }

    public Guid UsuarioId { get; private set; }
    public string UsuarioNome { get; private set; } = string.Empty;
    public Guid AlteradoPorId { get; private set; }
    public string AlteradoPorNome { get; private set; } = string.Empty;
    public string Item { get; private set; } = string.Empty;
    public string Anterior { get; private set; } = string.Empty;
    public string Novo { get; private set; } = string.Empty;

    public static AuditoriaAcesso Criar(
        Guid usuarioId,
        string usuarioNome,
        Guid alteradoPorId,
        string alteradoPorNome,
        string item,
        string anterior,
        string novo)
        => new()
        {
            UsuarioId = usuarioId,
            UsuarioNome = usuarioNome,
            AlteradoPorId = alteradoPorId,
            AlteradoPorNome = alteradoPorNome,
            Item = item,
            Anterior = anterior,
            Novo = novo
        };
}
