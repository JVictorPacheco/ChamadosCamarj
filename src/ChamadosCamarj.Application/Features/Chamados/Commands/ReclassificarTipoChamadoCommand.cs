using MediatR;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

/// <summary>Atendente/Admin corrige o tipo do chamado (spec area-e-tipo-do-chamado AC-11).</summary>
public record ReclassificarTipoChamadoCommand(
    Guid Id,
    Guid NovoTipoId,
    Guid? UsuarioId = null,
    string UsuarioNome = "Sistema"
) : IRequest, IRequerAcessoChamado
{
    Guid IRequerAcessoChamado.ChamadoId => Id;
    AcaoChamado IRequerAcessoChamado.Acao => AcaoChamado.ReclassificarTipo;
}
