using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record AlterarPrioridadeChamadoCommand(
    Guid Id,
    string NovaPrioridade
) : IRequest;
