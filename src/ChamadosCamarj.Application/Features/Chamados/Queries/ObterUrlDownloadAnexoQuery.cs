using MediatR;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public record ObterUrlDownloadAnexoQuery(Guid ChamadoId, Guid AnexoId) : IRequest<string>, IRequerAcessoChamado
{
    AcaoChamado IRequerAcessoChamado.Acao => AcaoChamado.Ver;
}
