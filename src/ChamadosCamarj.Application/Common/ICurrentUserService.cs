namespace ChamadosCamarj.Application.Common;

/// <summary>
/// Identidade do usuário autenticado na requisição atual, extraída dos claims do JWT
/// (ver AutenticarGoogleCommandHandler para como o token é emitido).
/// </summary>
public interface ICurrentUserService
{
    Guid UsuarioId { get; }
    string Nome { get; }
    string Perfil { get; }
    Guid? GrupoId { get; }
}
