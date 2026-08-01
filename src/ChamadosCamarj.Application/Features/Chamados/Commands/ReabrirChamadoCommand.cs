using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record ReabrirChamadoCommand(Guid Id, Guid? UsuarioId = null, string UsuarioNome = "Sistema") : IRequest;
