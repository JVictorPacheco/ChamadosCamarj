using MediatR;

namespace ChamadosCamarj.Application.Features.Chat.Commands.DeletarMensagem;

public record DeletarMensagemCommand(
    Guid MensagemId,
    Guid UsuarioId = default,
    string UsuarioNome = "",
    string PerfilRequisitante = ""
) : IRequest;
