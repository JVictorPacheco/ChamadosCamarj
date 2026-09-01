using MediatR;

namespace ChamadosCamarj.Application.Features.Chat.Commands.EditarMensagem;

public record EditarMensagemCommand(
    Guid MensagemId,
    string NovoConteudo,
    Guid UsuarioId = default,
    string UsuarioNome = ""
) : IRequest;
