using MediatR;
using ChamadosCamarj.Application.Features.Grupos.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Grupos.Queries;

public class ListarGruposQueryHandler : IRequestHandler<ListarGruposQuery, IEnumerable<GrupoResponse>>
{
    private readonly IGrupoRepository _grupoRepository;

    public ListarGruposQueryHandler(IGrupoRepository grupoRepository)
    {
        _grupoRepository = grupoRepository;
    }

    public async Task<IEnumerable<GrupoResponse>> Handle(ListarGruposQuery request, CancellationToken cancellationToken)
    {
        var grupos = await _grupoRepository.ListarAsync(cancellationToken);

        return grupos
            .OrderBy(g => g.Nome)
            .Select(g => g.ToResponse());
    }
}
