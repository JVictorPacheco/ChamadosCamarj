using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record FecharChamadoCommand(Guid Id) : IRequest;
