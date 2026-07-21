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

        IEnumerable<Domain.Enums.StatusChamado>? statusEntre = null;
        if (request.Finalizados == true)
            statusEntre = [Domain.Enums.StatusChamado.Resolvido, Domain.Enums.StatusChamado.Fechado, Domain.Enums.StatusChamado.Cancelado];

        // O model binding do ASP.NET Core produz DateTime com Kind=Unspecified a partir da query
        // string, mas a coluna DataCriacao é "timestamp with time zone" no Postgres — só aceita UTC.
        // DataFim vira o fim do dia (23:59:59.999...), não a meia-noite, para incluir o dia inteiro
        // selecionado (senão filtrar "hoje até hoje" não retornaria nada criado depois da meia-noite).
        DateTime? dataInicio = request.DataInicio.HasValue
            ? DateTime.SpecifyKind(request.DataInicio.Value.Date, DateTimeKind.Utc)
            : null;
        DateTime? dataFim = request.DataFim.HasValue
            ? DateTime.SpecifyKind(request.DataFim.Value.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1)
            : null;

        var (items, total) = await _chamadoRepository.ListarAsync(
            request.Pagina,
            request.TamanhoPagina,
            status,
            prioridade,
            request.ResponsavelId,
            request.CategoriaId,
            request.Busca,
            request.SolicitanteEmail,
            statusEntre,
            dataInicio,
            dataFim,
            cancellationToken);

        return new PagedResult<ChamadoResponse>(
            items.Select(c => c.ToResponse()),
            total,
            request.Pagina,
            request.TamanhoPagina);
    }
}
