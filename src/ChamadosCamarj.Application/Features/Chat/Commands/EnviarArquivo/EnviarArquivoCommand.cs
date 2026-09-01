using MediatR;
using ChamadosCamarj.Application.Features.Chat.DTOs;

namespace ChamadosCamarj.Application.Features.Chat.Commands.EnviarArquivo;

public record EnviarArquivoCommand(
    Guid ConversaId,
    string NomeArquivoOriginal,
    string ContentType,
    Stream Conteudo,
    long TamanhoBytes,
    Guid UsuarioId = default,
    string UsuarioNome = ""
) : IRequest<ChatMensagemResponse>;
