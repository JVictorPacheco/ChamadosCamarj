using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record CancelarChamadoCommand(
    Guid Id,
    Domain.Enums.MotivoEncerramento Motivo,
    string? MotivoOutro = null,
    Guid? UsuarioId = null,
    string UsuarioNome = "Sistema") : IRequest;
