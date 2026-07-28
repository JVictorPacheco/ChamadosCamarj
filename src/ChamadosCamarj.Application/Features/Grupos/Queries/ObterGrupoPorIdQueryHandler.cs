using MediatR;
using ChamadosCamarj.Application.Features.Grupos.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Grupos.Queries;

public class ObterGrupoPorIdQueryHandler : IRequestHandler<ObterGrupoPorIdQuery, GrupoResponse?>
{
    private readonly IGrupoRepository _grupoRepository;

    public ObterGrupoPorIdQueryHandler(IGrupoRepository grupoRepository)
    {
        _grupoRepository = grupoRepository;
    }

    public async Task<GrupoResponse?> Handle(ObterGrupoPorIdQuery request, CancellationToken cancellationToken)
    {
        var grupo = await _grupoRepository.ObterPorIdAsync(request.Id, cancellationToken);
        return grupo?.ToResponse();
    }
}
