using System.Text.Json;
using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Commands.DefinirChatPerfil;

public class DefinirChatPerfilCommandHandler : IRequestHandler<DefinirChatPerfilCommand>
{
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IChatHistoricoRepository _historicoRepository;
    private readonly IChatConversaRepository _conversaRepository;
    private readonly IChatMensagemRepository _mensagemRepository;
    private readonly IMediator _mediator;

    public DefinirChatPerfilCommandHandler(
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IChatHistoricoRepository historicoRepository,
        IChatConversaRepository conversaRepository,
        IChatMensagemRepository mensagemRepository,
        IMediator mediator)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _historicoRepository = historicoRepository;
        _conversaRepository = conversaRepository;
        _mensagemRepository = mensagemRepository;
        _mediator = mediator;
    }

    public async Task Handle(DefinirChatPerfilCommand request, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var usuario = await _usuarioPerfilRepository.ObterPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario is null)
            throw new NotFoundException("Usuário", request.UsuarioId);

        var perfilAnterior = usuario.ChatPerfil;
        if (perfilAnterior == request.ChatPerfil)
            return;

        usuario.DefinirChatPerfil(request.ChatPerfil);
        await _usuarioPerfilRepository.AtualizarAsync(usuario, cancellationToken);

        var revogou = perfilAnterior != ChatPerfil.SemAcesso && request.ChatPerfil == ChatPerfil.SemAcesso;
        var acao = revogou ? ChatAcao.AcessoRevogado : ChatAcao.AcessoConcedido;

        var detalhe = JsonSerializer.Serialize(new
        {
            perfilAnterior = perfilAnterior.ToString(),
            perfilNovo = request.ChatPerfil.ToString(),
            usuarioAlvo = usuario.Nome
        });

        await _historicoRepository.AdicionarAsync(
            ChatHistorico.Criar(request.AdminId, request.AdminNome, acao, detalhe),
            cancellationToken);

        if (revogou)
        {
            // Mensagem de sistema em cada conversa ativa do usuário + notifica o próprio usuário.
            var conversas = await _conversaRepository.ListarConversasComUsuarioAsync(usuario.Id, cancellationToken);
            foreach (var conversa in conversas)
            {
                var mensagemSistema = ChatMensagem.CriarSistema(
                    conversa.Id, $"{usuario.Nome} teve o acesso ao chat revogado");
                await _mensagemRepository.AdicionarAsync(mensagemSistema, cancellationToken);
            }

            await _mediator.Publish(new ChatAcessoRevogadoNotification(usuario.Id), cancellationToken);
        }
    }
}
