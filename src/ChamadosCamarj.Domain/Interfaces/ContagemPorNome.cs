namespace ChamadosCamarj.Domain.Interfaces;

/// <summary>Contagem agrupada por área ou por tipo (Dashboard).</summary>
public record ContagemPorNome(string Nome, Guid? Id, int Quantidade);
