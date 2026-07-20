using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record AdicionarAnexoCommand(
    Guid ChamadoId,
    Guid? ComentarioId,
    string NomeArquivoOriginal,
    string ContentType,
    Stream Conteudo,
    long TamanhoBytes,
    Guid? UsuarioId = null,
    string UsuarioNome = "Sistema"
) : IRequest<AnexoResponse>;
