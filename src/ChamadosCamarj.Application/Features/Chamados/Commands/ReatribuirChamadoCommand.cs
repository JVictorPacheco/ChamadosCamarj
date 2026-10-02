using MediatR;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record ReatribuirChamadoCommand(
    Guid Id,
    Guid NovoResponsavelId,
    string NovoResponsavelNome,
    Guid? UsuarioId = null,
    string UsuarioNome = "Sistema"
) : IRequest, IRequerAcessoChamado
{
    Guid IRequerAcessoChamado.ChamadoId => Id;
    AcaoChamado IRequerAcessoChamado.Acao => AcaoChamado.Reatribuir;
}
