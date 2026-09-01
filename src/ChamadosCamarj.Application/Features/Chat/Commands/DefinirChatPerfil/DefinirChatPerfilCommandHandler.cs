using System.Text.Json;
using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Mappings;
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
        // AC-46/47: simétrico à revogação — concedeu só conta como "restauração" quando a pessoa
        // vinha de SemAcesso. Uma troca lateral (Participante -> CriadorDeGrupo, por exemplo) não
        // é restauração — ela nunca perdeu acesso às conversas, não faz sentido anunciar "voltou".
        var restaurou = perfilAnterior == ChatPerfil.SemAcesso && request.ChatPerfil != ChatPerfil.SemAcesso;
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

        if (revogou || restaurou)
        {
            var textoSistema = revogou
                ? $"{usuario.Nome} teve o acesso ao chat revogado"
                : $"{usuario.Nome} teve o acesso ao chat restaurado";

            // Mensagem de sistema em cada conversa onde a pessoa já era participante — revogar não
            // remove ninguém de grupo nenhum, só bloqueia a tela; os vínculos continuam intactos.
            var conversas = await _conversaRepository.ListarConversasComUsuarioAsync(usuario.Id, cancellationToken);
            foreach (var conversa in conversas)
            {
                var mensagemSistema = ChatMensagem.CriarSistema(conversa.Id, textoSistema);
                await _mensagemRepository.AdicionarAsync(mensagemSistema, cancellationToken);

                // Sem isso, os outros participantes só veriam essa mensagem de sistema ao
                // recarregar a conversa em vez de em tempo real.
                var response = mensagemSistema.ToResponse(usuario.Id);
                // review-fase9-independente.md #3: já temos os participantes desta conversa em mãos
                // (ListarConversasComUsuarioAsync inclui Participantes) — passa direto pra evitar o
                // handler de notificação refazer a mesma busca a cada conversa deste loop.
                var destinatarioIds = conversa.Participantes.Where(p => p.Ativo).Select(p => p.UsuarioId);
                await _mediator.Publish(new ChatNovaMensagemNotification(conversa.Id, response, destinatarioIds), cancellationToken);
            }
        }

        if (revogou)
            await _mediator.Publish(new ChatAcessoRevogadoNotification(usuario.Id), cancellationToken);

        // AC-47: canal global (ChamadosHub), não o ChatHub — o usuário afetado pode não estar (e no
        // caso de revogação, nunca está) com uma conexão ativa ao ChatHub, que só existe na tela /chat.
        await _mediator.Publish(new ChatPerfilAtualizadoNotification(usuario.Id, request.ChatPerfil), cancellationToken);
    }
}
