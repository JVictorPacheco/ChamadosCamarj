using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public class ListarChamadosQueryHandler : IRequestHandler<ListarChamadosQuery, IEnumerable<ChamadoResponse>>
{
    private readonly IChamadoRepository _chamadoRepository;

    public ListarChamadosQueryHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task<IEnumerable<ChamadoResponse>> Handle(ListarChamadosQuery request, CancellationToken cancellationToken)
    {
        var chamados = await _chamadoRepository.ObterTodosAsync(cancellationToken);

        // Filtros
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<Domain.Enums.StatusChamado>(request.Status, true, out var status))
            chamados = chamados.Where(c => c.Status == status);

        if (!string.IsNullOrWhiteSpace(request.Prioridade) && Enum.TryParse<Domain.Enums.PrioridadeChamado>(request.Prioridade, true, out var prioridade))
            chamados = chamados.Where(c => c.Prioridade == prioridade);

        if (request.ResponsavelId.HasValue)
            chamados = chamados.Where(c => c.ResponsavelId == request.ResponsavelId);

        if (request.CategoriaId.HasValue)
            chamados = chamados.Where(c => c.CategoriaId == request.CategoriaId);

        if (!string.IsNullOrWhiteSpace(request.Busca))
            chamados = chamados.Where(c =>
                c.Titulo.Contains(request.Busca, StringComparison.OrdinalIgnoreCase) ||
                c.Descricao.Contains(request.Busca, StringComparison.OrdinalIgnoreCase));

        // Paginação
        chamados = chamados
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina);

        return chamados.Select(c => c.ToResponse());
    }
}
