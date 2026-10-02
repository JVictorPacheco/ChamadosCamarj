using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public record ListarAnexosQuery(Guid ChamadoId) : IRequest<IEnumerable<AnexoResponse>>, IRequerAcessoChamado
{
    AcaoChamado IRequerAcessoChamado.Acao => AcaoChamado.Ver;
}
