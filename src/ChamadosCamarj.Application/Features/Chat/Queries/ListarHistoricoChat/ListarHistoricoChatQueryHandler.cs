using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Queries.ListarHistoricoChat;

public class ListarHistoricoChatQueryHandler : IRequestHandler<ListarHistoricoChatQuery, IEnumerable<ChatHistoricoResponse>>
{
    private readonly IChatHistoricoRepository _historicoRepository;

    public ListarHistoricoChatQueryHandler(IChatHistoricoRepository historicoRepository)
    {
        _historicoRepository = historicoRepository;
    }

    public async Task<IEnumerable<ChatHistoricoResponse>> Handle(ListarHistoricoChatQuery request, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var historicos = request.ConversaId.HasValue
            ? await _historicoRepository.ListarPorConversaAsync(request.ConversaId.Value, cancellationToken)
            : await _historicoRepository.ListarTodasAsync(cancellationToken);

        return historicos.Select(h => h.ToResponse()).ToList();
    }
}
