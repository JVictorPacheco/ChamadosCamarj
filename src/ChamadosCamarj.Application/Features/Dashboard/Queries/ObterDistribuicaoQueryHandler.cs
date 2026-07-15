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
        var aguardando = await _chamadoRepository.ContarPorStatusAsync(StatusChamado.Aberto, cancellationToken);
        var assumido = await _chamadoRepository.ContarPorStatusAsync(StatusChamado.EmAndamento, cancellationToken);
        var resolvido = await _chamadoRepository.ContarPorStatusAsync(StatusChamado.Resolvido, cancellationToken);
        var encerrado = await _chamadoRepository.ContarPorStatusAsync(StatusChamado.Fechado, cancellationToken);
        var cancelado = await _chamadoRepository.ContarPorStatusAsync(StatusChamado.Cancelado, cancellationToken);

        return new DistribuicaoResponse(aguardando, assumido, resolvido, encerrado, cancelado);
    }
}
