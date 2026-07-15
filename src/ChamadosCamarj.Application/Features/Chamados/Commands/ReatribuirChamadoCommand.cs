using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record ReatribuirChamadoCommand(
    Guid Id,
    Guid NovoResponsavelId,
    string NovoResponsavelNome,
    Guid? UsuarioId = null,
    string UsuarioNome = "Sistema"
) : IRequest;
