using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record ComentarChamadoCommand(
    Guid ChamadoId,
    string Autor,
    string Conteudo,
    bool Interno = false
) : IRequest<ComentarioResponse>, IRequerAcessoChamado
{
    AcaoChamado IRequerAcessoChamado.Acao => Interno ? AcaoChamado.ComentarInterno : AcaoChamado.ComentarPublico;
}
