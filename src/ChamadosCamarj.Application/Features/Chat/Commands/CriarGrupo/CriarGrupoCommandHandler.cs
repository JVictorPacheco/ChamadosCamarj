using System.Text.Json;
using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Commands.CriarGrupo;

public class CriarGrupoCommandHandler : IRequestHandler<CriarGrupoCommand, ChatConversaResponse>
{
    private readonly IChatConversaRepository _conversaRepository;
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IChatHistoricoRepository _historicoRepository;
    private readonly IMediator _mediator;

    public CriarGrupoCommandHandler(
        IChatConversaRepository conversaRepository,
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IChatHistoricoRepository historicoRepository,
        IMediator mediator)
    {
        _conversaRepository = conversaRepository;
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _historicoRepository = historicoRepository;
        _mediator = mediator;
    }

    public async Task<ChatConversaResponse> Handle(CriarGrupoCommand request, CancellationToken cancellationToken)
    {
        var criador = await _usuarioPerfilRepository.ObterPorIdAsync(request.UsuarioId, cancellationToken);
        if (criador is null)
            throw new NotFoundException("Usuário", request.UsuarioId);

        ChatPerfilGuard.ExigirCriadorDeGrupo(criador.ChatPerfil);

        var idsParticipantes = request.ParticipanteIds.Where(id => id != request.UsuarioId).Distinct().ToList();
        if (idsParticipantes.Count < 2)
            throw new BadRequestException("Um grupo precisa de ao menos 2 participantes além do criador.");

        var conversa = ChatConversa.CriarGrupo(request.Nome, request.UsuarioId);
        conversa.AdicionarParticipante(new ChatParticipante(conversa.Id, criador.Id, criador.Nome));

        var nomesParticipantes = new List<string>();
        foreach (var participanteId in idsParticipantes)
        {
            var participante = await _usuarioPerfilRepository.ObterPorIdAsync(participanteId, cancellationToken);
            if (participante is null)
                throw new NotFoundException("Usuário", participanteId);
            if (participante.ChatPerfil == ChatPerfil.SemAcesso)
                throw new BadRequestException($"O usuário {participante.Nome} não tem acesso ao chat.");

            conversa.AdicionarParticipante(new ChatParticipante(conversa.Id, participante.Id, participante.Nome));
            nomesParticipantes.Add(participante.Nome);
        }

        await _conversaRepository.AdicionarAsync(conversa, cancellationToken);

        var detalhe = JsonSerializer.Serialize(new
        {
            nome = request.Nome,
            participantes = nomesParticipantes
        });

        await _historicoRepository.AdicionarAsync(
            ChatHistorico.Criar(request.UsuarioId, request.UsuarioNome, ChatAcao.GrupoCriado, detalhe, conversa.Id),
            cancellationToken);

        var response = new ChatConversaResponse(conversa.Id, conversa.Tipo, conversa.Nome, null, null, 0);

        var todosIds = conversa.Participantes.Select(p => p.UsuarioId).ToList();
        await _mediator.Publish(new ChatNovaConversaNotification(conversa.Id, todosIds, response), cancellationToken);

        return response;
    }
}
