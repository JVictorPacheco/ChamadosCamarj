using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record FecharChamadoCommand(
    Guid Id,
    Domain.Enums.MotivoEncerramento Motivo = Domain.Enums.MotivoEncerramento.Resolvido,
    string? MotivoOutro = null,
    string? Observacao = null,
    Guid? UsuarioId = null,
    string UsuarioNome = "Sistema") : IRequest;
