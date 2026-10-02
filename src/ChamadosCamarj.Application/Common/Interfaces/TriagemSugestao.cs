namespace ChamadosCamarj.Application.Common.Interfaces;

public class TriagemSugestao
{
    public Guid? AreaId { get; init; }
    public string? AreaNome { get; init; }
    public Guid? TipoId { get; init; }
    public string? TipoNome { get; init; }
    public int Confianca { get; init; }

    public bool TemSugestao => AreaId.HasValue || TipoId.HasValue;
}
