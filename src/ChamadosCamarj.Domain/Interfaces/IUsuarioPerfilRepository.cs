using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Domain.Interfaces;

public interface IUsuarioPerfilRepository
{
    Task<UsuarioPerfil?> ObterPorEmailAsync(string email, CancellationToken ct);
    Task<UsuarioPerfil?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<UsuarioPerfil>> ListarAsync(CancellationToken ct);
    Task AdicionarAsync(UsuarioPerfil usuario, CancellationToken ct);
    Task AtualizarAsync(UsuarioPerfil usuario, CancellationToken ct);
}
