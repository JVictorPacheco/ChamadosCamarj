using MediatR;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record RemoverAnexoCommand(Guid ChamadoId, Guid AnexoId, Guid RequisitanteId, string PerfilRequisitante) : IRequest, IRequerAcessoChamado
{
    AcaoChamado IRequerAcessoChamado.Acao => AcaoChamado.Anexar;
}
