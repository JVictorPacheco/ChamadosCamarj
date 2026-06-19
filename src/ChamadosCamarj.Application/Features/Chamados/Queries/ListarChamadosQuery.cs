using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public record ListarChamadosQuery(
    int Pagina = 1,
    int TamanhoPagina = 10,
    string? Status = null,
    string? Prioridade = null,
    Guid? ResponsavelId = null,
    Guid? CategoriaId = null,
    string? Busca = null
) : IRequest<PagedResult<ChamadoResponse>>;
