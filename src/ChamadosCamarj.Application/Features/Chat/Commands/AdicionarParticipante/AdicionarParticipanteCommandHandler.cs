using System.Text.Json;
using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Commands.AdicionarParticipante;

public class AdicionarParticipanteCommandHandler : IRequestHandler<AdicionarParticipanteCommand>
{
    private readonly IChatConversaRepository _conversaRepository;
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IChatMensagemRepository _mensagemRepository;
    private readonly IChatHistoricoRepository _historicoRepository;
    private readonly IMediator _mediator;

    public AdicionarParticipanteCommandHandler(
        IChatConversaRepository conversaRepository,
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IChatMensagemRepository mensagemRepository,
        IChatHistoricoRepository historicoRepository,
        IMediator mediator)
    {
        _conversaRepository = conversaRepository;
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _mensagemRepository = mensagemRepository;
        _historicoRepository = historicoRepository;
        _mediator = mediator;
    }

    public async Task Handle(AdicionarParticipanteCommand request, CancellationToken cancellationToken)
    {
        var conversa = await _conversaRepository.ObterPorIdAsync(request.ConversaId, cancellationToken);
        if (conversa is null)
            throw new NotFoundException("Conversa", request.ConversaId);
        if (conversa.Tipo != ChatConversaTipo.Grupo)
            throw new BadRequestException("Só é possível gerenciar participantes em conversas de grupo.");

        ChatPerfilGuard.ExigirCriadorDaConversaOuAdmin(conversa.CriadoPorId, request.RequisitanteId, request.RequisitantePerfil);

        var usuario = await _usuarioPerfilRepository.ObterPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario is null)
            throw new NotFoundException("Usuário", request.UsuarioId);
        if (usuario.ChatPerfil == ChatPerfil.SemAcesso)
            throw new BadRequestException($"O usuário {usuario.Nome} não tem acesso ao chat.");

        var existente = conversa.Participantes.FirstOrDefault(p => p.UsuarioId == request.UsuarioId);
        if (existente is { Ativo: true })
            throw new ConflictException($"{usuario.Nome} já é participante deste grupo.");

        if (existente is not null)
            existente.Reativar();
        else
            conversa.AdicionarParticipante(new ChatParticipante(conversa.Id, usuario.Id, usuario.Nome));

        await _conversaRepository.AtualizarAsync(conversa, cancellationToken);

        var detalhe = JsonSerializer.Serialize(new { participante = usuario.Nome });
        await _historicoRepository.AdicionarAsync(
            ChatHistorico.Criar(request.RequisitanteId, request.RequisitanteNome, ChatAcao.ParticipanteAdicionado, detalhe, conversa.Id),
            cancellationToken);

        var mensagemSistema = ChatMensagem.CriarSistema(
            conversa.Id, $"{request.RequisitanteNome} adicionou {usuario.Nome}");
        await _mensagemRepository.AdicionarAsync(mensagemSistema, cancellationToken);

        var response = mensagemSistema.ToResponse(request.RequisitanteId);
        await _mediator.Publish(new ChatNovaMensagemNotification(conversa.Id, response), cancellationToken);
        await _mediator.Publish(new ChatParticipanteAdicionadoNotification(conversa.Id, usuario.Id), cancellationToken);
    }
}
