using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chat.DTOs;

public record ChatConversaResponse(
    Guid Id,
    ChatConversaTipo Tipo,
    string? Nome,
    string? UltimaMensagem,
    DateTime? UltimaMensagemEm,
    int NaoLidas
);
