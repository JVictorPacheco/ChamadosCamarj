using System.Text.Json;
using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Commands.AdicionarReacao;

public class AdicionarReacaoCommandHandler : IRequestHandler<AdicionarReacaoCommand>
{
    private readonly IChatMensagemRepository _mensagemRepository;
    private readonly IChatConversaRepository _conversaRepository;
    private readonly IChatHistoricoRepository _historicoRepository;
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IMediator _mediator;

    public AdicionarReacaoCommandHandler(
        IChatMensagemRepository mensagemRepository,
        IChatConversaRepository conversaRepository,
        IChatHistoricoRepository historicoRepository,
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IMediator mediator)
    {
        _mensagemRepository = mensagemRepository;
        _conversaRepository = conversaRepository;
        _historicoRepository = historicoRepository;
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _mediator = mediator;
    }

    public async Task Handle(AdicionarReacaoCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioPerfilRepository.ObterPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario is null)
            throw new NotFoundException("Usuário", request.UsuarioId);

        ChatPerfilGuard.ExigirAcesso(usuario.ChatPerfil);

        var mensagem = await _mensagemRepository.ObterPorIdAsync(request.MensagemId, cancellationToken);
        if (mensagem is null)
            throw new NotFoundException("Mensagem", request.MensagemId);

        var participante = await _conversaRepository.ObterParticipanteAsync(mensagem.ConversaId, request.UsuarioId, cancellationToken);
        if (participante is null || !participante.Ativo)
            throw new ForbiddenException("Você não participa desta conversa.");

        var existente = await _mensagemRepository.ObterReacaoAsync(request.MensagemId, request.UsuarioId, request.Emoji, cancellationToken);

        ChatAcao acao;
        if (existente is not null)
        {
            await _mensagemRepository.RemoverReacaoAsync(existente, cancellationToken);
            acao = ChatAcao.ReacaoRemovida;
        }
        else
        {
            var reacao = new ChatMensagemReacao(request.MensagemId, request.UsuarioId, request.UsuarioNome, request.Emoji);
            // Toggle não-atômico: em cliques simultâneos, ObterReacaoAsync pode retornar null nas duas
            // requisições. AdicionarReacaoAsync trata a violação do índice único como no-op (a reação já
            // existe), evitando 500. Nesse caso mantemos ReacaoAdicionada para o log.
            await _mensagemRepository.AdicionarReacaoAsync(reacao, cancellationToken);
            acao = ChatAcao.ReacaoAdicionada;
        }

        var detalhe = JsonSerializer.Serialize(new { emoji = request.Emoji });
        await _historicoRepository.AdicionarAsync(
            ChatHistorico.Criar(request.UsuarioId, request.UsuarioNome, acao, detalhe, mensagem.ConversaId, mensagem.Id),
            cancellationToken);

        var atualizada = await _mensagemRepository.ObterPorIdComReacoesAsync(request.MensagemId, cancellationToken);
        var reacoes = atualizada is null
            ? Enumerable.Empty<Features.Chat.DTOs.ChatReacaoResponse>()
            : atualizada.Reacoes.ToReacoesResponse(request.UsuarioId);

        await _mediator.Publish(new ChatReacaoAtualizadaNotification(mensagem.ConversaId, request.MensagemId, reacoes), cancellationToken);
    }
}
