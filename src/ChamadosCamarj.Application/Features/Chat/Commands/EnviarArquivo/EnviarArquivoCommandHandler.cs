using System.Text.Json;
using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Commands.EnviarArquivo;

public class EnviarArquivoCommandHandler : IRequestHandler<EnviarArquivoCommand, ChatMensagemResponse>
{
    private readonly IChatConversaRepository _conversaRepository;
    private readonly IChatMensagemRepository _mensagemRepository;
    private readonly IChatHistoricoRepository _historicoRepository;
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IChatStorageService _storageService;
    private readonly IMediator _mediator;

    public EnviarArquivoCommandHandler(
        IChatConversaRepository conversaRepository,
        IChatMensagemRepository mensagemRepository,
        IChatHistoricoRepository historicoRepository,
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IChatStorageService storageService,
        IMediator mediator)
    {
        _conversaRepository = conversaRepository;
        _mensagemRepository = mensagemRepository;
        _historicoRepository = historicoRepository;
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _storageService = storageService;
        _mediator = mediator;
    }

    public async Task<ChatMensagemResponse> Handle(EnviarArquivoCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioPerfilRepository.ObterPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario is null)
            throw new NotFoundException("Usuário", request.UsuarioId);

        ChatPerfilGuard.ExigirAcesso(usuario.ChatPerfil);

        var participante = await _conversaRepository.ObterParticipanteAsync(request.ConversaId, request.UsuarioId, cancellationToken);
        if (participante is null || !participante.Ativo)
            throw new ForbiddenException("Você não participa desta conversa.");

        var extensao = Path.GetExtension(request.NomeArquivoOriginal);
        var mensagemId = Guid.NewGuid();
        var caminho = $"chat/{request.ConversaId}/{mensagemId}/{Guid.NewGuid()}{extensao}";

        await _storageService.UploadAsync(caminho, request.ContentType, request.Conteudo, cancellationToken);

        var mensagem = ChatMensagem.CriarArquivo(
            request.ConversaId, request.UsuarioId, request.UsuarioNome,
            request.NomeArquivoOriginal, caminho, request.ContentType, request.TamanhoBytes);

        try
        {
            await _mensagemRepository.AdicionarAsync(mensagem, cancellationToken);
        }
        catch
        {
            await _storageService.RemoverAsync(caminho, CancellationToken.None);
            throw;
        }

        var detalhe = JsonSerializer.Serialize(new
        {
            nomeArquivo = request.NomeArquivoOriginal,
            tipoArquivo = request.ContentType,
            tamanhoBytes = request.TamanhoBytes
        });

        await _historicoRepository.AdicionarAsync(
            ChatHistorico.Criar(request.UsuarioId, request.UsuarioNome, ChatAcao.ArquivoEnviado, detalhe, request.ConversaId, mensagem.Id),
            cancellationToken);

        var response = mensagem.ToResponse(request.UsuarioId);
        await _mediator.Publish(new ChatNovaMensagemNotification(request.ConversaId, response), cancellationToken);

        return response;
    }
}
