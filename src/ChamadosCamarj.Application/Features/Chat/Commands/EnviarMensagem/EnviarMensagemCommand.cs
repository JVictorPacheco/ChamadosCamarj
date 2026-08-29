using MediatR;
using ChamadosCamarj.Application.Features.Chat.DTOs;

namespace ChamadosCamarj.Application.Features.Chat.Commands.EnviarMensagem;

public record EnviarMensagemCommand(
    Guid ConversaId,
    string Conteudo,
    Guid? RespostaParaMensagemId = null,
    Guid UsuarioId = default,
    string UsuarioNome = ""
) : IRequest<ChatMensagemResponse>;
