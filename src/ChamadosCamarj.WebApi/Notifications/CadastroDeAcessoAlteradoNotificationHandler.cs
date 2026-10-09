using MediatR;
using Microsoft.AspNetCore.SignalR;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.WebApi.Hubs;

namespace ChamadosCamarj.WebApi.Notifications;

/// <summary>
/// Perfil, equipe ou situação da conta mudaram (spec perfil-no-cadastro D7, AC-13..15). A conexão de tempo
/// real fica aberta e o token só é conferido ao conectar; por isso o servidor reajusta, na hora, os grupos de
/// avisos que dependem do perfil (rebaixado deixa de receber alertas restritos, promovido passa a receber) e
/// derruba as conexões de quem foi desativado. A reconexão de conta desativada é recusada na autenticação.
/// </summary>
public class CadastroDeAcessoAlteradoNotificationHandler : INotificationHandler<CadastroDeAcessoAlteradoNotification>
{
    private readonly IHubContext<ChamadosHub> _chamadosHub;
    private readonly ConexoesTempoReal _conexoes;

    public CadastroDeAcessoAlteradoNotificationHandler(IHubContext<ChamadosHub> chamadosHub, ConexoesTempoReal conexoes)
    {
        _chamadosHub = chamadosHub;
        _conexoes = conexoes;
    }

    public async Task Handle(CadastroDeAcessoAlteradoNotification notification, CancellationToken cancellationToken)
    {
        var conexoes = _conexoes.DoUsuario(notification.UsuarioId);

        if (!notification.Ativo)
        {
            foreach (var conexao in conexoes)
                conexao.Contexto.Abort();
            return;
        }

        var devidos = ChamadosHub.GruposDoPerfil(notification.Perfil.ToString());
        foreach (var conexao in conexoes.Where(c => EhDoChamadosHub(c)))
        {
            foreach (var grupo in ChamadosHub.GruposPorPerfil)
            {
                if (devidos.Contains(grupo))
                    await _chamadosHub.Groups.AddToGroupAsync(conexao.ConnectionId, grupo, cancellationToken);
                else
                    await _chamadosHub.Groups.RemoveFromGroupAsync(conexao.ConnectionId, grupo, cancellationToken);
            }
        }
    }

    // As conexões do ChatHub não têm grupos que dependam de perfil: só o ChamadosHub é reajustado.
    private static bool EhDoChamadosHub(ConexaoAberta conexao) => conexao.Hub == nameof(ChamadosHub);
}
