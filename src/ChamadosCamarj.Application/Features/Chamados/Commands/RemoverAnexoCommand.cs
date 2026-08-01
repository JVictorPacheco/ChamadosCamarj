using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record RemoverAnexoCommand(Guid ChamadoId, Guid AnexoId, Guid RequisitanteId, string PerfilRequisitante) : IRequest;
