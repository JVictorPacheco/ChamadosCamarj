using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record ResolverChamadoCommand(Guid Id) : IRequest;
