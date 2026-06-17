using MediatR;
using ChamadosCamarj.Application.Features.Categorias.DTOs;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Categorias.Queries;

public class ListarCategoriasQueryHandler : IRequestHandler<ListarCategoriasQuery, IEnumerable<CategoriaResponse>>
{
    private readonly ICategoriaRepository _categoriaRepository;

    public ListarCategoriasQueryHandler(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IEnumerable<CategoriaResponse>> Handle(ListarCategoriasQuery request, CancellationToken cancellationToken)
    {
        var categorias = request.ApenasAtivas == true
            ? await _categoriaRepository.ObterAtivasAsync(cancellationToken)
            : await _categoriaRepository.ObterTodasAsync(cancellationToken);

        return categorias.Select(c => new CategoriaResponse(c.Id, c.Nome, c.Descricao, c.Ativa));
    }
}
