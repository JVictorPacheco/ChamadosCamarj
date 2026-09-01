using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chat.DTOs;

public record ChatParticipanteInfo(Guid UsuarioId, string UsuarioNome);

public record ChatConversaDetalheResponse(
    Guid Id,
    ChatConversaTipo Tipo,
    string? Nome,
    Guid CriadoPorId,
    IEnumerable<ChatParticipanteInfo> Participantes
);
