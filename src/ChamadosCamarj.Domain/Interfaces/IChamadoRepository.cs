using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Domain.Interfaces;

public interface IChamadoRepository
{
    // Comandos
    Task<Chamado> AdicionarAsync(Chamado chamado, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Chamado chamado, CancellationToken cancellationToken = default);
    Task AdicionarComentarioAsync(Comentario comentario, CancellationToken cancellationToken = default);

    // Consultas
    Task<Chamado?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comentario>> ObterComentariosPorChamadoAsync(Guid chamadoId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterPorStatusAsync(StatusChamado status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterPorSolicitanteAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterPorResponsavelAsync(Guid responsavelId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterAtrasadosAsync(CancellationToken cancellationToken = default);

    Task<(IEnumerable<Chamado> Items, int Total)> ListarAsync(
        int pagina,
        int tamanhoPagina,
        StatusChamado? status = null,
        PrioridadeChamado? prioridade = null,
        Guid? responsavelId = null,
        Guid? categoriaId = null,
        string? busca = null,
        string? solicitanteEmail = null,
        CancellationToken cancellationToken = default);

    // Verificações
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);

    // Dashboard / Métricas
    Task<int> ContarPorStatusAsync(StatusChamado status, CancellationToken cancellationToken = default);
    Task<int> ContarResolvidosHojeAsync(CancellationToken cancellationToken = default);
    Task<double?> ObterTempoMedioResolucaoHorasAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> ContarPorCategoriaAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> ContarPorPrioridadeAsync(CancellationToken cancellationToken = default);
    Task<List<(DateTime Data, int Abertos, int Resolvidos)>> ObterTendenciaAsync(int dias, CancellationToken cancellationToken = default);
}
