using MediatR;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Queries.ListarPresencas;

public class ListarPresencasQueryHandler : IRequestHandler<ListarPresencasQuery, IEnumerable<ChatPresencaResponse>>
{
    private readonly IChatPresencaRepository _presencaRepository;
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;

    public ListarPresencasQueryHandler(
        IChatPresencaRepository presencaRepository,
        IUsuarioPerfilRepository usuarioPerfilRepository)
    {
        _presencaRepository = presencaRepository;
        _usuarioPerfilRepository = usuarioPerfilRepository;
    }

    public async Task<IEnumerable<ChatPresencaResponse>> Handle(ListarPresencasQuery request, CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioPerfilRepository.ListarAsync(cancellationToken);
        var presencas = (await _presencaRepository.ListarTodasAsync(cancellationToken))
            .ToDictionary(p => p.UsuarioId);

        return usuarios
            .Where(u => u.Ativo)
            .Select(u => presencas.TryGetValue(u.Id, out var presenca)
                ? new ChatPresencaResponse(u.Id, u.Nome, presenca.Status)
                : new ChatPresencaResponse(u.Id, u.Nome, StatusPresenca.Offline))
            .OrderBy(p => p.UsuarioNome)
            .ToList();
    }
}
