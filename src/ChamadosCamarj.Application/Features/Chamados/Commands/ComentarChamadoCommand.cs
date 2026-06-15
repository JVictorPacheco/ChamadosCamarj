using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record ComentarChamadoCommand(
    Guid ChamadoId,
    string Autor,
    string Conteudo,
    bool Interno = false
) : IRequest;
