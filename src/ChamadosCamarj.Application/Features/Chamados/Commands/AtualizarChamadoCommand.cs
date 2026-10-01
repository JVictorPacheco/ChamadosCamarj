using MediatR;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record AtualizarChamadoCommand(
    Guid Id,
    string Titulo,
    string Descricao
) : IRequest, IRequerAcessoChamado
{
    Guid IRequerAcessoChamado.ChamadoId => Id;
    AcaoChamado IRequerAcessoChamado.Acao => AcaoChamado.Editar;
}
