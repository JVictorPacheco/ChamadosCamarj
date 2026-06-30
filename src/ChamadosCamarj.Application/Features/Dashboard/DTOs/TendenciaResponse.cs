namespace ChamadosCamarj.Application.Features.Dashboard.DTOs;

public record TendenciaItem(string Data, int Abertos, int Resolvidos);

public record TendenciaResponse(List<TendenciaItem> Items);
