using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Domain.Interfaces;

public interface IChamadoRepository
{
    // Comandos
    Task<Chamado> AdicionarAsync(Chamado chamado, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Chamado chamado, CancellationToken cancellationToken = default);
    Task AdicionarComentarioAsync(Comentario comentario, CancellationToken cancellationToken = default);
    Task AdicionarAnexoAsync(Anexo anexo, CancellationToken cancellationToken = default);
    Task RemoverAnexoAsync(Guid anexoId, CancellationToken cancellationToken = default);

    // Consultas
    Task<Chamado?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Chamado?> ObterPorIdComTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comentario>> ObterComentariosPorChamadoAsync(Guid chamadoId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Anexo>> ObterAnexosPorChamadoAsync(Guid chamadoId, CancellationToken cancellationToken = default);
    Task<Anexo?> ObterAnexoPorIdAsync(Guid anexoId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterPorStatusAsync(StatusChamado status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterPorSolicitanteAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterPorResponsavelAsync(Guid responsavelId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Chamado>> ObterAtrasadosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista só os chamados que <paramref name="acesso"/> pode ver. Os demais filtros são
    /// aplicados por cima dessa regra — nenhum deles amplia a visibilidade.
    /// </summary>
    Task<(IEnumerable<Chamado> Items, int Total)> ListarAsync(
        ContextoAcesso acesso,
        int pagina,
        int tamanhoPagina,
        StatusChamado? status = null,
        PrioridadeChamado? prioridade = null,
        Guid? responsavelId = null,
        Guid? categoriaId = null,
        string? busca = null,
        string? solicitanteEmail = null,
        IEnumerable<StatusChamado>? statusEntre = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        Domain.Enums.MotivoEncerramento? motivoEncerramento = null,
        CancellationToken cancellationToken = default);

    // Verificações
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// True se o chamado existe E <paramref name="acesso"/> pode vê-lo — mesma regra do ListarAsync.
    /// </summary>
    Task<bool> PodeVerAsync(Guid chamadoId, ContextoAcesso acesso, CancellationToken cancellationToken = default);

    // Dashboard / Métricas — contam só o que o usuário pode ver
    Task<int> ContarPorStatusAsync(StatusChamado status, CancellationToken cancellationToken = default);
    Task<(int TotalResolvidos, int DentroPrazo)> ContarSlaComplianceAsync(ContextoAcesso acesso, DateTime inicio, DateTime fim, CancellationToken cancellationToken = default);
    Task<Dictionary<StatusChamado, int>> ContarPorStatusAgrupadoAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default);
    Task<int> ContarResolvidosHojeAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default);
    Task<double?> ObterTempoMedioResolucaoHorasAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default);
    Task<List<CategoriaContagem>> ContarPorCategoriaAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> ContarPorPrioridadeAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default);
}
