using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace ChamadosCamarj.WebApi.Hubs;

/// <summary>
/// Conexões de tempo real abertas, por pessoa (spec perfil-no-cadastro D6). O token só é conferido quando a
/// conexão abre; para reajustar os grupos de avisos de quem mudou de perfil, ou derrubar a conexão de quem
/// foi desativado, o servidor precisa saber quais conexões são dessa pessoa. Vale para os dois hubs.
/// </summary>
public class ConexoesTempoReal
{
    private readonly ConcurrentDictionary<string, ConexaoAberta> _conexoes = new();

    /// <summary>Guarda a conexão. Sem identificador de usuário válido (não deveria ocorrer com token) não guarda.</summary>
    public void Registrar(HubCallerContext contexto, string hub)
    {
        if (!Guid.TryParse(contexto.UserIdentifier, out var usuarioId))
            return;

        _conexoes[contexto.ConnectionId] = new ConexaoAberta(contexto.ConnectionId, usuarioId, hub, contexto);
    }

    public void Remover(string connectionId) => _conexoes.TryRemove(connectionId, out _);

    public IReadOnlyList<ConexaoAberta> DoUsuario(Guid usuarioId) =>
        _conexoes.Values.Where(c => c.UsuarioId == usuarioId).ToList();
}

public record ConexaoAberta(string ConnectionId, Guid UsuarioId, string Hub, HubCallerContext Contexto);
