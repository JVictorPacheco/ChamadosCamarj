using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public record ListarComentariosQuery(Guid ChamadoId, string? PerfilUsuario = null) : IRequest<IEnumerable<ComentarioResponse>>, IRequerAcessoChamado
{
    AcaoChamado IRequerAcessoChamado.Acao => AcaoChamado.Ver;
}
