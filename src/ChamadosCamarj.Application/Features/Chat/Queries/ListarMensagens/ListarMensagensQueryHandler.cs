using MediatR;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Queries.ListarMensagens;

public class ListarMensagensQueryHandler : IRequestHandler<ListarMensagensQuery, PagedResult<ChatMensagemResponse>>
{
    private const int TamanhoPagina = 20;

    private readonly IChatConversaRepository _conversaRepository;
    private readonly IChatMensagemRepository _mensagemRepository;

    public ListarMensagensQueryHandler(
        IChatConversaRepository conversaRepository,
        IChatMensagemRepository mensagemRepository)
    {
        _conversaRepository = conversaRepository;
        _mensagemRepository = mensagemRepository;
    }

    public async Task<PagedResult<ChatMensagemResponse>> Handle(ListarMensagensQuery request, CancellationToken cancellationToken)
    {
        var participante = await _conversaRepository.ObterParticipanteAsync(request.ConversaId, request.UsuarioId, cancellationToken);
        if (participante is null || !participante.Ativo)
            throw new ForbiddenException("Você não participa desta conversa.");

        var pagina = request.Pagina < 1 ? 1 : request.Pagina;
        var total = await _mensagemRepository.ContarPorConversaAsync(request.ConversaId, cancellationToken);
        var mensagens = await _mensagemRepository.ListarPorConversaAsync(request.ConversaId, pagina, TamanhoPagina, cancellationToken);

        var items = mensagens
            .Select(m => m.ToResponse(request.UsuarioId))
            .ToList();

        return new PagedResult<ChatMensagemResponse>(items, total, pagina, TamanhoPagina);
    }
}
