using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
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
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;

    public ListarMensagensQueryHandler(
        IChatConversaRepository conversaRepository,
        IChatMensagemRepository mensagemRepository,
        IUsuarioPerfilRepository usuarioPerfilRepository)
    {
        _conversaRepository = conversaRepository;
        _mensagemRepository = mensagemRepository;
        _usuarioPerfilRepository = usuarioPerfilRepository;
    }

    public async Task<PagedResult<ChatMensagemResponse>> Handle(ListarMensagensQuery request, CancellationToken cancellationToken)
    {
        // review-fase9-independente.md #2 — ver mesmo comentário em ListarConversasQueryHandler.
        var usuario = await _usuarioPerfilRepository.ObterPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario is null)
            throw new NotFoundException("Usuário", request.UsuarioId);
        ChatPerfilGuard.ExigirAcesso(usuario.ChatPerfil);

        var participante = await _conversaRepository.ObterParticipanteAsync(request.ConversaId, request.UsuarioId, cancellationToken);
        if (participante is null || !participante.Ativo)
            throw new ForbiddenException("Você não participa desta conversa.");

        var pagina = request.Pagina < 1 ? 1 : request.Pagina;
        var total = await _mensagemRepository.ContarPorConversaAsync(request.ConversaId, cancellationToken);
        var mensagens = await _mensagemRepository.ListarPorConversaAsync(request.ConversaId, pagina, TamanhoPagina, cancellationToken);

        // Busca em lote o conteúdo das mensagens citadas (evita N+1) para popular a citação/reply.
        var idsCitados = mensagens
            .Where(m => m.RespostaParaMensagemId.HasValue)
            .Select(m => m.RespostaParaMensagemId!.Value);
        var conteudosCitados = await _mensagemRepository.ObterConteudosPorIdsAsync(idsCitados, cancellationToken);

        var items = mensagens
            .Select(m =>
            {
                string? respostaConteudo = null;
                if (m.RespostaParaMensagemId.HasValue &&
                    conteudosCitados.TryGetValue(m.RespostaParaMensagemId.Value, out var conteudoOriginal))
                {
                    respostaConteudo = conteudoOriginal ?? "[arquivo]";
                }

                return m.ToResponse(request.UsuarioId, respostaConteudo);
            })
            .ToList();

        return new PagedResult<ChatMensagemResponse>(items, total, pagina, TamanhoPagina);
    }
}
