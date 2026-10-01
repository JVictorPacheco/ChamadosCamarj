using MediatR;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record CancelarChamadoCommand(
    Guid Id,
    Domain.Enums.MotivoEncerramento Motivo,
    string? MotivoOutro = null,
    string? Observacao = null,
    Guid? UsuarioId = null,
    string UsuarioNome = "Sistema") : IRequest, IRequerAcessoChamado
{
    Guid IRequerAcessoChamado.ChamadoId => Id;
    AcaoChamado IRequerAcessoChamado.Acao => AcaoChamado.Cancelar;
}
