using MediatR;
using ChamadosCamarj.Application.Features.Chat.DTOs;

namespace ChamadosCamarj.Application.Features.Chat.Commands.CriarGrupo;

public record CriarGrupoCommand(
    string Nome,
    IReadOnlyList<Guid> ParticipanteIds,
    Guid UsuarioId = default,
    string UsuarioNome = ""
) : IRequest<ChatConversaResponse>;
