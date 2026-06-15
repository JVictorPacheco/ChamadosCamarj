using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record AtualizarChamadoCommand(
    Guid Id,
    string Titulo,
    string Descricao
) : IRequest;
