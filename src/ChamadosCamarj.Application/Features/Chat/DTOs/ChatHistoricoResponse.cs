using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chat.DTOs;

public record ChatHistoricoResponse(
    Guid Id,
    Guid UsuarioId,
    string UsuarioNome,
    ChatAcao Acao,
    string? Detalhe,
    Guid? ConversaId,
    Guid? MensagemId,
    DateTime DataHora
);
