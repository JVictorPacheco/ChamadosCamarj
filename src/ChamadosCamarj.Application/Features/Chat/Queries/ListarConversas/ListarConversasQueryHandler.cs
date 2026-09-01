using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chat.Queries.ListarConversas;

public class ListarConversasQueryHandler : IRequestHandler<ListarConversasQuery, IEnumerable<ChatConversaResponse>>
{
    private readonly IChatConversaRepository _conversaRepository;
    private readonly IChatMensagemRepository _mensagemRepository;
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;

    public ListarConversasQueryHandler(
        IChatConversaRepository conversaRepository,
        IChatMensagemRepository mensagemRepository,
        IUsuarioPerfilRepository usuarioPerfilRepository)
    {
        _conversaRepository = conversaRepository;
        _mensagemRepository = mensagemRepository;
        _usuarioPerfilRepository = usuarioPerfilRepository;
    }

    public async Task<IEnumerable<ChatConversaResponse>> Handle(ListarConversasQuery request, CancellationToken cancellationToken)
    {
        // review-fase9-independente.md #2: revogar acesso ao chat não removia ninguém de conversa
        // nenhuma — sem esta guarda, um usuário com ChatPerfil=SemAcesso continuava conseguindo
        // listar conversas, ler mensagens e baixar anexos via API, só não via link na sidebar.
        var usuario = await _usuarioPerfilRepository.ObterPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario is null)
            throw new NotFoundException("Usuário", request.UsuarioId);
        ChatPerfilGuard.ExigirAcesso(usuario.ChatPerfil);

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
