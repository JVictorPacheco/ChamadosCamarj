using MediatR;

namespace ChamadosCamarj.Application.Features.Chat.Commands.AdicionarReacao;

public record AdicionarReacaoCommand(
    Guid MensagemId,
    string Emoji,
    Guid UsuarioId = default,
    string UsuarioNome = ""
) : IRequest;
