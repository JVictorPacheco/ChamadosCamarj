using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Common.Autorizacao;

/// <summary>
/// Confere se o usuário logado tem um módulo (spec controle-de-acesso AC-12). Como o ChatPerfil, o acesso
/// não vive no token: lê o cadastro a cada pedido, então uma mudança feita pelo Admin vale na hora.
/// </summary>
public class ModuloGuard
{
    public const string MensagemPerfilMudou = "Seu perfil mudou. Entre novamente para continuar.";

    private readonly IUsuarioPerfilRepository _usuarios;
    private readonly ICurrentUserService _currentUser;

    public ModuloGuard(IUsuarioPerfilRepository usuarios, ICurrentUserService currentUser)
    {
        _usuarios = usuarios;
        _currentUser = currentUser;
    }

    public async Task ExigirAsync(ModuloSistema modulo, string mensagem, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.ObterPorIdAsync(_currentUser.UsuarioId, cancellationToken);
        if (usuario is null || !usuario.Ativo || !usuario.ModulosEfetivos().HasFlag(modulo))
            throw new ForbiddenException(mensagem);

        // O escopo dos números (quem vê o quê) vem do perfil do token. Se o cadastro já tem outro perfil,
        // recusa: nunca entregar números no escopo de um perfil antigo (review R-07).
        if (!string.Equals(usuario.Perfil.ToString(), _currentUser.Perfil, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException(MensagemPerfilMudou);
    }
}
