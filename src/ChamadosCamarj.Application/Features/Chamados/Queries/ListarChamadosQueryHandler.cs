using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public class ListarChamadosQueryHandler : IRequestHandler<ListarChamadosQuery, PagedResult<ChamadoResponse>>
{
    private readonly IChamadoRepository _chamadoRepository;

    public ListarChamadosQueryHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task<PagedResult<ChamadoResponse>> Handle(ListarChamadosQuery request, CancellationToken cancellationToken)
    {
        Domain.Enums.StatusChamado? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<Domain.Enums.StatusChamado>(request.Status, ignoreCase: true, out var statusParsed))
            status = statusParsed;

        Domain.Enums.PrioridadeChamado? prioridade = null;
        if (!string.IsNullOrWhiteSpace(request.Prioridade) &&
            Enum.TryParse<Domain.Enums.PrioridadeChamado>(request.Prioridade, ignoreCase: true, out var prioridadeParsed))
            prioridade = prioridadeParsed;

        var (items, total) = await _chamadoRepository.ListarAsync(
            request.Pagina,
            request.TamanhoPagina,
            status,
            prioridade,
            request.ResponsavelId,
            request.CategoriaId,
            request.Busca,
            cancellationToken);

        return new PagedResult<ChamadoResponse>(
            items.Select(c => c.ToResponse()),
            total,
            request.Pagina,
            request.TamanhoPagina);
    }
}
