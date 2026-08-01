namespace ChamadosCamarj.Application.Common.Interfaces;

public class TriagemSugestao
{
    public Guid? CategoriaId { get; init; }
    public string? CategoriaNome { get; init; }
    public Guid? GrupoId { get; init; }
    public string? GrupoNome { get; init; }
    public int Confianca { get; init; }

    public bool TemSugestao => CategoriaId.HasValue;
}
