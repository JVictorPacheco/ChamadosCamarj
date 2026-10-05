using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Relatorios;

/// <summary>
/// De quem são os números do relatório mensal (autorizacao-chamados AC-21 + controle-de-acesso AC-07):
/// Admin vê tudo (pode filtrar por um atendente); Atendente só os próprios, qualquer que seja o pedido;
/// Solicitante com o módulo só os chamados que ele vê. Extraído do controller para ser testável (review R-08).
/// </summary>
public static class RelatorioEscopo
{
    public static async Task<(Guid? ResponsavelId, IReadOnlyCollection<Guid>? IdsVisiveis)> ResolverAsync(
        ContextoAcesso acesso, Guid? responsavelIdPedido, IChamadoRepository chamados, CancellationToken cancellationToken)
    {
        return acesso.Perfil switch
        {
            Perfil.Admin => (responsavelIdPedido, null),
            Perfil.Atendente => (acesso.UsuarioId, null),
            _ => (null, await chamados.ListarIdsVisiveisAsync(acesso, cancellationToken))
        };
    }
}
