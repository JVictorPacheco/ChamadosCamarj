using MediatR;
using ChamadosCamarj.Application.Features.Dashboard.DTOs;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Dashboard.Queries;

public class ObterTendenciaQueryHandler : IRequestHandler<ObterTendenciaQuery, TendenciaResponse>
{
    private readonly IChamadoRepository _chamadoRepository;

    public ObterTendenciaQueryHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task<TendenciaResponse> Handle(ObterTendenciaQuery request, CancellationToken cancellationToken)
    {
        var tendencia = await _chamadoRepository.ObterTendenciaAsync(request.Dias, cancellationToken);

        return new TendenciaResponse(
            tendencia.Select(t => new TendenciaItem(
                t.Data.ToString("yyyy-MM-dd"),
                t.Abertos,
                t.Resolvidos
            )).ToList()
        );
    }
}
