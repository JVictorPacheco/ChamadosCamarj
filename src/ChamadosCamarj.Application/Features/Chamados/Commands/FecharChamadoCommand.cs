using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record FecharChamadoCommand(Guid Id, Guid? UsuarioId = null, string UsuarioNome = "Sistema") : IRequest;
