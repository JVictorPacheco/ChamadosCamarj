using MediatR;

namespace ChamadosCamarj.Application.Features.Usuarios.Commands;

public record RedefinirSenhaCommand(
    Guid Id,
    string NovaSenha,
    string? PerfilRequisitante = null
) : IRequest;
