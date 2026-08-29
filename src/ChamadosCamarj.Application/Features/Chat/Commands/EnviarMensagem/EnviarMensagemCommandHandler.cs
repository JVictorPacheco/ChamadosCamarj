using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Commands.EnviarMensagem;

public class EnviarMensagemCommandHandler : IRequestHandler<EnviarMensagemCommand, ChatMensagemResponse>
{
    private readonly IChatConversaRepository _conversaRepository;
    private readonly IChatMensagemRepository _mensagemRepository;
    private readonly IChatHistoricoRepository _historicoRepository;
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IMediator _mediator;

    public EnviarMensagemCommandHandler(
        IChatConversaRepository conversaRepository,
        IChatMensagemRepository mensagemRepository,
        IChatHistoricoRepository historicoRepository,
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IMediator mediator)
    {
        _conversaRepository = conversaRepository;
        _mensagemRepository = mensagemRepository;
        _historicoRepository = historicoRepository;
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _mediator = mediator;
    }

    public async Task<ChatMensagemResponse> Handle(EnviarMensagemCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioPerfilRepository.ObterPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario is null)
            throw new NotFoundException("Usuário", request.UsuarioId);

        ChatPerfilGuard.ExigirAcesso(usuario.ChatPerfil);

        var participante = await _conversaRepository.ObterParticipanteAsync(request.ConversaId, request.UsuarioId, cancellationToken);
        if (participante is null || !participante.Ativo)
            throw new ForbiddenException("Você não participa desta conversa.");

        string? respostaConteudo = null;
        if (request.RespostaParaMensagemId.HasValue)
        {
            var original = await _mensagemRepository.ObterPorIdAsync(request.RespostaParaMensagemId.Value, cancellationToken);
            if (original is null)
                throw new NotFoundException("Mensagem", request.RespostaParaMensagemId.Value);
            if (original.ConversaId != request.ConversaId)
                throw new BadRequestException("Mensagem citada não pertence a esta conversa.");
            respostaConteudo = original.Conteudo;
        }

        var mensagem = ChatMensagem.CriarTexto(
            request.ConversaId, request.UsuarioId, request.UsuarioNome, request.Conteudo, request.RespostaParaMensagemId);

        await _mensagemRepository.AdicionarAsync(mensagem, cancellationToken);

        await _historicoRepository.AdicionarAsync(
            ChatHistorico.Criar(request.UsuarioId, request.UsuarioNome, Domain.Enums.ChatAcao.MensagemEnviada, null, request.ConversaId, mensagem.Id),
            cancellationToken);

        var response = mensagem.ToResponse(request.UsuarioId, respostaConteudo);
        await _mediator.Publish(new ChatNovaMensagemNotification(request.ConversaId, response), cancellationToken);

        return response;
    }
}
