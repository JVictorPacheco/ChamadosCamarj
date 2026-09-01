using System.Text.Json;
using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Commands.RemoverParticipante;

public class RemoverParticipanteCommandHandler : IRequestHandler<RemoverParticipanteCommand>
{
    private readonly IChatConversaRepository _conversaRepository;
    private readonly IChatMensagemRepository _mensagemRepository;
    private readonly IChatHistoricoRepository _historicoRepository;
    private readonly IMediator _mediator;

    public RemoverParticipanteCommandHandler(
        IChatConversaRepository conversaRepository,
        IChatMensagemRepository mensagemRepository,
        IChatHistoricoRepository historicoRepository,
        IMediator mediator)
    {
        _conversaRepository = conversaRepository;
        _mensagemRepository = mensagemRepository;
        _historicoRepository = historicoRepository;
        _mediator = mediator;
    }

    public async Task Handle(RemoverParticipanteCommand request, CancellationToken cancellationToken)
    {
        var conversa = await _conversaRepository.ObterPorIdAsync(request.ConversaId, cancellationToken);
        if (conversa is null)
            throw new NotFoundException("Conversa", request.ConversaId);
        if (conversa.Tipo != ChatConversaTipo.Grupo)
            throw new BadRequestException("Só é possível gerenciar participantes em conversas de grupo.");

        ChatPerfilGuard.ExigirCriadorDaConversaOuAdmin(conversa.CriadoPorId, request.RequisitanteId, request.RequisitantePerfil);

        var participante = conversa.Participantes.FirstOrDefault(p => p.UsuarioId == request.UsuarioId && p.Ativo);
        if (participante is null)
            throw new NotFoundException("Participante", request.UsuarioId);

        participante.Desativar();
        await _conversaRepository.AtualizarParticipanteAsync(participante, cancellationToken);

        var detalhe = JsonSerializer.Serialize(new { participante = participante.UsuarioNome });
        await _historicoRepository.AdicionarAsync(
            ChatHistorico.Criar(request.RequisitanteId, request.RequisitanteNome, ChatAcao.ParticipanteRemovido, detalhe, conversa.Id),
            cancellationToken);

        var mensagemSistema = ChatMensagem.CriarSistema(
            conversa.Id, $"{request.RequisitanteNome} removeu {participante.UsuarioNome}");
        await _mensagemRepository.AdicionarAsync(mensagemSistema, cancellationToken);

        var response = mensagemSistema.ToResponse(request.RequisitanteId);
        await _mediator.Publish(new ChatNovaMensagemNotification(conversa.Id, response), cancellationToken);
        await _mediator.Publish(new ChatParticipanteRemovidoNotification(conversa.Id, participante.UsuarioId), cancellationToken);
    }
}
