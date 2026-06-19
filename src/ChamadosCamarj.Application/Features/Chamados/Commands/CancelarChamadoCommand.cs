using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record CancelarChamadoCommand(Guid Id) : IRequest;
