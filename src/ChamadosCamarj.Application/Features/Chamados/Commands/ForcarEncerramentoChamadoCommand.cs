using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record ForcarEncerramentoChamadoCommand(
    Guid Id,
    string Motivo,
    Guid? UsuarioId = null,
    string UsuarioNome = "Sistema",
    string? PerfilRequisitante = null) : IRequest;
