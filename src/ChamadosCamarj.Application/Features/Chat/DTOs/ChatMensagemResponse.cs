using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chat.DTOs;

public record ChatMensagemResponse(
    Guid Id,
    Guid ConversaId,
    Guid AutorId,
    string AutorNome,
    string? Conteudo,
    ChatMensagemTipo Tipo,
    bool Deletada,
    DateTime? EditadaEm,
    Guid? RespostaParaMensagemId,
    string? RespostaConteudo,
    string? NomeArquivo,
    string? TipoArquivo,
    long? TamanhoBytes,
    IEnumerable<ChatReacaoResponse> Reacoes,
    DateTime DataCriacao
);
