namespace ChamadosCamarj.Application.Features.Chamados.DTOs;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int Total,
    int Pagina,
    int TamanhoPagina
)
{
    public int TotalPaginas => (int)Math.Ceiling((double)Total / TamanhoPagina);
    public bool TemProxima => Pagina < TotalPaginas;
    public bool TemAnterior => Pagina > 1;
}
