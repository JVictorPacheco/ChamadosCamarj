using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace ChamadosCamarj.WebApi.Hubs;

/// <summary>
/// Hub SignalR para notificações em tempo real do sistema de chamados.
/// </summary>
public class ChamadosHub : Hub
{
    /// <summary>Atendentes e Admins: recebem avisos operacionais, como o de comentário interno.</summary>
    public const string GrupoAtendimento = "Atendimento";

    /// <summary>Só Admins: recebem os alertas de SLA de todos os chamados (spec correcoes-pre-deploy AC-03).</summary>
    public const string GrupoAdmins = "Admins";

    /// <summary>Grupos que dependem do perfil: o servidor os reajusta quando o cadastro muda (perfil-no-cadastro D7).</summary>
    public static readonly IReadOnlyList<string> GruposPorPerfil = [GrupoAtendimento, GrupoAdmins];

    private readonly ConexoesTempoReal _conexoes;

    public ChamadosHub(ConexoesTempoReal conexoes) => _conexoes = conexoes;

    public override async Task OnConnectedAsync()
    {
        // Grupo padrão: todos os clientes recebem notificações globais
        await Groups.AddToGroupAsync(Context.ConnectionId, "Todos");

        // Grupos por perfil são definidos SÓ aqui, pelo servidor, a partir do token — o cliente
        // não escolhe em que grupo entra (spec correcoes-acesso-chamados AC-04). O perfil do token já vem
        // do cadastro (CadastroClaimsValidator).
        var perfil = Context.User?.FindFirst("perfil")?.Value;
        foreach (var grupo in GruposDoPerfil(perfil))
            await Groups.AddToGroupAsync(Context.ConnectionId, grupo);

        _conexoes.Registrar(Context, nameof(ChamadosHub));
        await base.OnConnectedAsync();
    }

    /// <summary>Grupos de avisos a que um perfil tem direito: Atendente/Admin no atendimento, só Admin nos alertas de SLA.</summary>
    public static IReadOnlyList<string> GruposDoPerfil(string? perfil)
    {
        var grupos = new List<string>();
        if (EhPerfilAtendimento(perfil))
            grupos.Add(GrupoAtendimento);
        if (EhPerfilAdmin(perfil))
            grupos.Add(GrupoAdmins);
        return grupos;
    }

    public static bool EhAtendimento(ClaimsPrincipal? usuario) => EhPerfilAtendimento(usuario?.FindFirst("perfil")?.Value);

    public static bool EhAdmin(ClaimsPrincipal? usuario) => EhPerfilAdmin(usuario?.FindFirst("perfil")?.Value);

    private static bool EhPerfilAtendimento(string? perfil) =>
        string.Equals(perfil, "Atendente", StringComparison.OrdinalIgnoreCase) || EhPerfilAdmin(perfil);

    private static bool EhPerfilAdmin(string? perfil) =>
        string.Equals(perfil, "Admin", StringComparison.OrdinalIgnoreCase);

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _conexoes.Remover(Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Todos");
        await base.OnDisconnectedAsync(exception);
    }
}
