using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public class ListarComentariosQueryHandler : IRequestHandler<ListarComentariosQuery, IEnumerable<ComentarioResponse>>
{
    private readonly IChamadoRepository _chamadoRepository;

    public ListarComentariosQueryHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task<IEnumerable<ComentarioResponse>> Handle(ListarComentariosQuery request, CancellationToken cancellationToken)
    {
        var comentarios = await _chamadoRepository.ObterComentariosPorChamadoAsync(request.ChamadoId, cancellationToken);
        return comentarios.Select(c => new ComentarioResponse(c.Id, c.Autor, c.Conteudo, c.Tipo, c.DataCriacao));
    }
}
