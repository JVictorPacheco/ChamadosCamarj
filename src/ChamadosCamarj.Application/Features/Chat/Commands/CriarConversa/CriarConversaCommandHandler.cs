using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Commands.CriarConversa;

public class CriarConversaCommandHandler : IRequestHandler<CriarConversaCommand, ChatConversaResponse>
{
    private readonly IChatConversaRepository _conversaRepository;
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IMediator _mediator;

    public CriarConversaCommandHandler(
        IChatConversaRepository conversaRepository,
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IMediator mediator)
    {
        _conversaRepository = conversaRepository;
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _mediator = mediator;
    }

    public async Task<ChatConversaResponse> Handle(CriarConversaCommand request, CancellationToken cancellationToken)
    {
        if (request.DestinatarioId == request.UsuarioId)
            throw new BadRequestException("Não é possível iniciar uma conversa consigo mesmo.");

        var solicitante = await _usuarioPerfilRepository.ObterPorIdAsync(request.UsuarioId, cancellationToken);
        if (solicitante is null)
            throw new NotFoundException("Usuário", request.UsuarioId);

        ChatPerfilGuard.ExigirAcesso(solicitante.ChatPerfil);

        var destinatario = await _usuarioPerfilRepository.ObterPorIdAsync(request.DestinatarioId, cancellationToken);
        if (destinatario is null)
            throw new NotFoundException("Usuário", request.DestinatarioId);

        if (destinatario.ChatPerfil == ChatPerfil.SemAcesso)
            throw new BadRequestException("O destinatário não tem acesso ao chat.");

        var existente = await _conversaRepository.ObterPrivadaEntreUsuariosAsync(request.UsuarioId, request.DestinatarioId, cancellationToken);
        if (existente is not null)
            return MapearResposta(existente, request.UsuarioId, destinatario.Nome);

        var conversa = ChatConversa.CriarPrivada(request.UsuarioId);
        conversa.AdicionarParticipante(new ChatParticipante(conversa.Id, request.UsuarioId, solicitante.Nome));
        conversa.AdicionarParticipante(new ChatParticipante(conversa.Id, request.DestinatarioId, destinatario.Nome));

        await _conversaRepository.AdicionarAsync(conversa, cancellationToken);

        var response = MapearResposta(conversa, request.UsuarioId, destinatario.Nome);

        var participanteIds = new[] { request.UsuarioId, request.DestinatarioId };
        await _mediator.Publish(
            new ChatNovaConversaNotification(conversa.Id, participanteIds, response),
            cancellationToken);

        return response;
    }

    private static ChatConversaResponse MapearResposta(ChatConversa conversa, Guid usuarioAtualId, string destinatarioNome)
    {
        // Em conversa privada o "nome" exibido é o nome do outro participante.
        var nomeExibicao = conversa.Nome;
        if (conversa.Tipo == ChatConversaTipo.Privada)
            nomeExibicao = destinatarioNome;

        return new ChatConversaResponse(conversa.Id, conversa.Tipo, nomeExibicao, null, null, 0);
    }
}
