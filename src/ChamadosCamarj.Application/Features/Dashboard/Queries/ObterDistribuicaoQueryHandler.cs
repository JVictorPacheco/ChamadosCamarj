using MediatR;
using ChamadosCamarj.Application.Features.Dashboard.DTOs;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Dashboard.Queries;

public class ObterDistribuicaoQueryHandler : IRequestHandler<ObterDistribuicaoQuery, DistribuicaoResponse>
{
    private readonly IChamadoRepository _chamadoRepository;

    public ObterDistribuicaoQueryHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task<DistribuicaoResponse> Handle(ObterDistribuicaoQuery request, CancellationToken cancellationToken)
    {
        var contagens = await _chamadoRepository.ContarPorStatusAgrupadoAsync(cancellationToken);

        return new DistribuicaoResponse(
            contagens.GetValueOrDefault(StatusChamado.Aberto),
            contagens.GetValueOrDefault(StatusChamado.EmAndamento),
            contagens.GetValueOrDefault(StatusChamado.Resolvido),
            contagens.GetValueOrDefault(StatusChamado.Fechado),
            contagens.GetValueOrDefault(StatusChamado.Cancelado)
        );
    }
}
