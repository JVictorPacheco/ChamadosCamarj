using MediatR;
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

    public ObterArquivoMensagemQueryHandler(
        IChatMensagemRepository mensagemRepository,
        IChatConversaRepository conversaRepository,
        IChatStorageService storageService)
    {
        _mensagemRepository = mensagemRepository;
        _conversaRepository = conversaRepository;
        _storageService = storageService;
    }

    public async Task<ChatArquivoResponse> Handle(ObterArquivoMensagemQuery request, CancellationToken cancellationToken)
    {
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
