using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Queries.ObterArquivoMensagem;

public class ObterArquivoMensagemQueryHandler : IRequestHandler<ObterArquivoMensagemQuery, ChatArquivoResponse>
{
    private const int ExpiracaoSegundos = 3600;

    private readonly IChatMensagemRepository _mensagemRepository;
    private readonly IChatConversaRepository _conversaRepository;
    private readonly IChatStorageService _storageService;
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;

    public ObterArquivoMensagemQueryHandler(
        IChatMensagemRepository mensagemRepository,
        IChatConversaRepository conversaRepository,
        IChatStorageService storageService,
        IUsuarioPerfilRepository usuarioPerfilRepository)
    {
        _mensagemRepository = mensagemRepository;
        _conversaRepository = conversaRepository;
        _storageService = storageService;
        _usuarioPerfilRepository = usuarioPerfilRepository;
    }

    public async Task<ChatArquivoResponse> Handle(ObterArquivoMensagemQuery request, CancellationToken cancellationToken)
    {
        // review-fase9-independente.md #2 — ver mesmo comentário em ListarConversasQueryHandler.
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

        if (mensagem.Tipo != ChatMensagemTipo.Arquivo || string.IsNullOrWhiteSpace(mensagem.CaminhoStorage))
            throw new BadRequestException("Esta mensagem não possui arquivo.");

        var urlAssinada = await _storageService.ObterUrlAssinadaAsync(mensagem.CaminhoStorage, ExpiracaoSegundos, cancellationToken);

        return new ChatArquivoResponse(
            mensagem.NomeArquivo ?? string.Empty,
            urlAssinada,
            mensagem.TipoArquivo ?? string.Empty,
            mensagem.TamanhoBytes ?? 0);
    }
}
