using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Queries.ObterConversa;

public class ObterConversaQueryHandler : IRequestHandler<ObterConversaQuery, ChatConversaDetalheResponse>
{
    private readonly IChatConversaRepository _conversaRepository;
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;

    public ObterConversaQueryHandler(
        IChatConversaRepository conversaRepository,
        IUsuarioPerfilRepository usuarioPerfilRepository)
    {
        _conversaRepository = conversaRepository;
        _usuarioPerfilRepository = usuarioPerfilRepository;
    }

    public async Task<ChatConversaDetalheResponse> Handle(ObterConversaQuery request, CancellationToken cancellationToken)
    {
        // review-fase9-independente.md #2 — ver mesmo comentário em ListarConversasQueryHandler.
        var usuario = await _usuarioPerfilRepository.ObterPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario is null)
            throw new NotFoundException("Usuário", request.UsuarioId);
        ChatPerfilGuard.ExigirAcesso(usuario.ChatPerfil);

        var conversa = await _conversaRepository.ObterPorIdAsync(request.ConversaId, cancellationToken);
        if (conversa is null)
            throw new NotFoundException("Conversa", request.ConversaId);

        var souParticipante = conversa.Participantes.Any(p => p.UsuarioId == request.UsuarioId && p.Ativo);
        if (!souParticipante)
            throw new ForbiddenException("Você não participa desta conversa.");

        var participantes = conversa.Participantes
            .Where(p => p.Ativo)
            .Select(p => new ChatParticipanteInfo(p.UsuarioId, p.UsuarioNome))
            .ToList();

        return new ChatConversaDetalheResponse(conversa.Id, conversa.Tipo, conversa.Nome, conversa.CriadoPorId, participantes);
    }
}
