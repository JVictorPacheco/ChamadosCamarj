using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Domain.Interfaces;

public interface IChatPresencaRepository
{
    Task<ChatPresenca?> ObterPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken);
    Task<IEnumerable<ChatPresenca>> ListarTodasAsync(CancellationToken cancellationToken);
    Task AdicionarOuAtualizarAsync(ChatPresenca presenca, CancellationToken cancellationToken);
    Task<IEnumerable<ChatPresenca>> ListarParaMarcarAusenteAsync(DateTime limite, CancellationToken cancellationToken);
    Task<IEnumerable<ChatPresenca>> ListarParaMarcarOfflineAsync(DateTime limite, CancellationToken cancellationToken);
    Task AtualizarVariasAsync(IEnumerable<ChatPresenca> presencas, CancellationToken cancellationToken);
}
