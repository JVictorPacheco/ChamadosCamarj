using ChamadosCamarj.Domain.Enums;
using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record AlterarStatusChamadoCommand(Guid Id, StatusChamado NovoStatus) : IRequest;
