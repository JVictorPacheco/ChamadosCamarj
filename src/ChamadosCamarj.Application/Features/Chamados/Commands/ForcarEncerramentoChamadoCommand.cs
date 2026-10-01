using MediatR;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record ForcarEncerramentoChamadoCommand(
    Guid Id,
    Domain.Enums.MotivoEncerramento Motivo,
    string? MotivoOutro = null,
    string? Observacao = null,
    Guid? UsuarioId = null,
    string UsuarioNome = "Sistema",
    string? PerfilRequisitante = null) : IRequest, IRequerAcessoChamado
{
    Guid IRequerAcessoChamado.ChamadoId => Id;
    AcaoChamado IRequerAcessoChamado.Acao => AcaoChamado.ForcarEncerramento;
}
