using MediatR;

namespace ChamadosCamarj.Application.Features.Chat.Commands.DefinirPreferenciaLeitura;

public record DefinirPreferenciaLeituraCommand(
    bool Mostrar,
    Guid UsuarioId = default
) : IRequest;
