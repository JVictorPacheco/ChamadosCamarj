using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record ForcarEncerramentoChamadoCommand(
    Guid Id,
    Domain.Enums.MotivoEncerramento Motivo,
    string? MotivoOutro = null,
    Guid? UsuarioId = null,
    string UsuarioNome = "Sistema",
    string? PerfilRequisitante = null) : IRequest;
