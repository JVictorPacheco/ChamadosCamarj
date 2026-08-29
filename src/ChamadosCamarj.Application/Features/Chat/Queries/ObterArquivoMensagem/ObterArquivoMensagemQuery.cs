using MediatR;
using ChamadosCamarj.Application.Features.Chat.DTOs;

namespace ChamadosCamarj.Application.Features.Chat.Queries.ObterArquivoMensagem;

public record ObterArquivoMensagemQuery(
    Guid MensagemId,
    Guid UsuarioId
) : IRequest<ChatArquivoResponse>;
