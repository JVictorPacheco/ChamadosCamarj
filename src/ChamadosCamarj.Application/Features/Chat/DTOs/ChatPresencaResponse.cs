using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chat.DTOs;

public record ChatPresencaResponse(
    Guid UsuarioId,
    string UsuarioNome,
    StatusPresenca Status
);
