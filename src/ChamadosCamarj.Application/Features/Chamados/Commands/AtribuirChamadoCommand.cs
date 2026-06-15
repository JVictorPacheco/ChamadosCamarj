using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record AtribuirChamadoCommand(
    Guid Id,
    Guid ResponsavelId,
    string ResponsavelNome
) : IRequest;
