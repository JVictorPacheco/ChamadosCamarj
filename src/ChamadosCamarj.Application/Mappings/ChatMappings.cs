using ChamadosCamarj.Application.Features.Chat.DTOs;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Application.Mappings;

public static class ChatMappings
{
    public static ChatMensagemResponse ToResponse(this ChatMensagem mensagem, Guid usuarioAtualId, string? respostaConteudo = null) =>
        new(
            mensagem.Id,
            mensagem.ConversaId,
            mensagem.AutorId,
            mensagem.AutorNome,
            mensagem.Conteudo,
            mensagem.Tipo,
            mensagem.Deletada,
            mensagem.EditadaEm,
            mensagem.RespostaParaMensagemId,
            respostaConteudo,
            mensagem.NomeArquivo,
            mensagem.TipoArquivo,
            mensagem.TamanhoBytes,
            mensagem.Reacoes.ToReacoesResponse(usuarioAtualId),
            mensagem.DataCriacao
        );

    public static IEnumerable<ChatReacaoResponse> ToReacoesResponse(this IEnumerable<ChatMensagemReacao> reacoes, Guid usuarioAtualId) =>
        reacoes
            .GroupBy(r => r.Emoji)
            .Select(g => new ChatReacaoResponse(
                g.Key,
                g.Count(),
                g.Any(r => r.UsuarioId == usuarioAtualId)))
            .ToList();

    public static ChatPresencaResponse ToResponse(this ChatPresenca presenca) =>
        new(presenca.UsuarioId, presenca.UsuarioNome, presenca.Status);

    public static ChatHistoricoResponse ToResponse(this ChatHistorico historico) =>
        new(
            historico.Id,
            historico.UsuarioId,
            historico.UsuarioNome,
            historico.Acao,
            historico.Detalhe,
            historico.ConversaId,
            historico.MensagemId,
            historico.DataCriacao
        );
}
