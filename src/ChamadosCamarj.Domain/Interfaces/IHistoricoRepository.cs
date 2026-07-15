using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Domain.Interfaces;

public record EventoRelatorioItem(
    Guid ChamadoId,
    AcaoHistorico Acao,
    DateTime DataHora,
    string CategoriaNome,
    Guid? ResponsavelId,
    string? ResponsavelNome,
    DateTime? DataConclusao,
    DateTime? DataLimite
);

public interface IHistoricoRepository
{
    Task<HistoricoEntrada> AdicionarAsync(HistoricoEntrada historico, CancellationToken cancellationToken = default);
    Task<IEnumerable<HistoricoEntrada>> ObterPorChamadoAsync(Guid chamadoId, CancellationToken cancellationToken = default);
    Task<HistoricoEntrada?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<EventoRelatorioItem>> ObterEventosParaRelatorioAsync(
        IEnumerable<AcaoHistorico> acoes,
        DateTime inicio,
        DateTime fimExclusivo,
        CancellationToken cancellationToken = default);
}
