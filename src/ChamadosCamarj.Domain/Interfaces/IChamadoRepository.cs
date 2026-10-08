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
        Guid? areaId = null,
        Guid? tipoId = null,
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

    /// <summary>
    /// Ids dos Atendentes ativos que podem ver o chamado — mesma regra do PodeVerAsync
    /// (spec correcoes-pre-deploy AC-01/AC-02, alerta de SLA).
    /// </summary>
    Task<IReadOnlyList<Guid>> ListarAtendentesQuePodemVerAsync(Guid chamadoId, CancellationToken cancellationToken = default);

    /// <summary>Ids de todos os chamados que <paramref name="acesso"/> pode ver — mesma regra do ListarAsync
    /// (spec controle-de-acesso: relatório do Solicitante com o módulo).</summary>
    Task<IReadOnlyCollection<Guid>> ListarIdsVisiveisAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default);

    /// <summary>True se o comentário existe e pertence ao chamado (spec correcoes-pre-deploy AC-06/AC-07).</summary>
    Task<bool> ComentarioPertenceAoChamadoAsync(Guid comentarioId, Guid chamadoId, CancellationToken cancellationToken = default);

    /// <summary>Versão atual do chamado (ver VersaoChamado), ou null se não existe. Spec correcoes-pre-deploy AC-09.</summary>
    Task<string?> ObterVersaoAsync(Guid chamadoId, CancellationToken cancellationToken = default);

    // Dashboard / Métricas — contam só o que o usuário pode ver
    Task<int> ContarPorStatusAsync(StatusChamado status, CancellationToken cancellationToken = default);
    Task<(int TotalResolvidos, int DentroPrazo)> ContarSlaComplianceAsync(ContextoAcesso acesso, DateTime inicio, DateTime fim, CancellationToken cancellationToken = default);
    Task<Dictionary<StatusChamado, int>> ContarPorStatusAgrupadoAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default);
    Task<int> ContarResolvidosHojeAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default);
    Task<double?> ObterTempoMedioResolucaoHorasAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default);
    Task<List<ContagemPorNome>> ContarPorAreaAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default);
    Task<List<ContagemPorNome>> ContarPorTipoAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> ContarPorPrioridadeAsync(ContextoAcesso acesso, CancellationToken cancellationToken = default);
}
