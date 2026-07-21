using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Domain.Enums;

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
        
        // Filtrar comentários internos: Solicitante só vê públicos, Admin/Atendente veem todos
        var comentariosFiltrados = comentarios.Where(c =>
        {
            if (c.Tipo == TipoComentario.Publico)
                return true;

            // Comentários internos só são vistos por Admin ou Atendente
            return request.PerfilUsuario is "Admin" or "Atendente";
        });

        return comentariosFiltrados.Select(c => new ComentarioResponse(c.Id, c.Autor, c.Conteudo, c.Tipo, c.DataCriacao));
    }
}
