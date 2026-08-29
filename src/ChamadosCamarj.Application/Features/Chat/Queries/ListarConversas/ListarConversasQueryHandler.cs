using MediatR;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Queries.ListarConversas;

public class ListarConversasQueryHandler : IRequestHandler<ListarConversasQuery, IEnumerable<ChatConversaResponse>>
{
    private readonly IChatConversaRepository _conversaRepository;
    private readonly IChatMensagemRepository _mensagemRepository;

    public ListarConversasQueryHandler(
        IChatConversaRepository conversaRepository,
        IChatMensagemRepository mensagemRepository)
    {
        _conversaRepository = conversaRepository;
        _mensagemRepository = mensagemRepository;
    }

    public async Task<IEnumerable<ChatConversaResponse>> Handle(ListarConversasQuery request, CancellationToken cancellationToken)
    {
        var conversas = (await _conversaRepository.ListarPorUsuarioAsync(request.UsuarioId, cancellationToken)).ToList();

        var conversaIds = conversas.Select(c => c.Id).ToList();

        var ultimasMensagens = await _mensagemRepository.ObterUltimasMensagensPorConversasAsync(conversaIds, cancellationToken);

        var leiturasPorConversa = conversas
            .Select(c => (
                ConversaId: c.Id,
                UltimaLeituraEm: c.Participantes.FirstOrDefault(p => p.UsuarioId == request.UsuarioId)?.UltimaLeituraEm))
            .ToList();

        var naoLidasPorConversa = await _mensagemRepository.ContarNaoLidasPorConversasAsync(
            leiturasPorConversa, request.UsuarioId, cancellationToken);

        var resultado = new List<ChatConversaResponse>();
        foreach (var conversa in conversas)
        {
            var ultima = ultimasMensagens.GetValueOrDefault(conversa.Id);
            var naoLidas = naoLidasPorConversa.GetValueOrDefault(conversa.Id);

            var nomeExibicao = conversa.Nome;
            if (conversa.Tipo == ChatConversaTipo.Privada)
            {
                var outro = conversa.Participantes.FirstOrDefault(p => p.UsuarioId != request.UsuarioId);
                nomeExibicao = outro?.UsuarioNome;
            }

            var ultimoTexto = ultima is null
                ? null
                : ultima.Deletada ? "[mensagem removida]"
                : ultima.Tipo == ChatMensagemTipo.Arquivo ? ultima.NomeArquivo
                : ultima.Conteudo;

            resultado.Add(new ChatConversaResponse(
                conversa.Id,
                conversa.Tipo,
                nomeExibicao,
                ultimoTexto,
                ultima?.DataCriacao,
                naoLidas));
        }

        return resultado
            .OrderByDescending(c => c.UltimaMensagemEm ?? DateTime.MinValue)
            .ToList();
    }
}
