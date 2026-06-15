using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public class ObterChamadoPorIdQueryHandler : IRequestHandler<ObterChamadoPorIdQuery, ChamadoResponse?>
{
    private readonly IChamadoRepository _chamadoRepository;

    public ObterChamadoPorIdQueryHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task<ChamadoResponse?> Handle(ObterChamadoPorIdQuery request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken);
        return chamado?.ToResponse();
    }
}
