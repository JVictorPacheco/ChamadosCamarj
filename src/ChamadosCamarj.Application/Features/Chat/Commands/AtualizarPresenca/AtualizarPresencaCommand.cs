using MediatR;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chat.Commands.AtualizarPresenca;

/// <summary>
/// Upsert de presença. Status null significa heartbeat (marca Online e atualiza timestamp).
/// </summary>
public record AtualizarPresencaCommand(
    StatusPresenca? Status = null,
    Guid UsuarioId = default,
    string UsuarioNome = ""
) : IRequest;
