using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record ResolverChamadoCommand(Guid Id, Guid? UsuarioId = null, string UsuarioNome = "Sistema") : IRequest;
