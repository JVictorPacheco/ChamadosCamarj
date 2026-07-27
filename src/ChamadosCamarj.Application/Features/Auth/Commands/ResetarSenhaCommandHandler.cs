using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Auth.Commands;

public class ResetarSenhaCommandHandler : IRequestHandler<ResetarSenhaCommand, bool>
{
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IPasswordHasher<UsuarioPerfil> _passwordHasher;
    private readonly AuthSettings _authSettings;

    public ResetarSenhaCommandHandler(
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IPasswordHasher<UsuarioPerfil> passwordHasher,
        IOptions<AuthSettings> authSettings)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _passwordHasher = passwordHasher;
        _authSettings = authSettings.Value;
    }

    public async Task<bool> Handle(ResetarSenhaCommand request, CancellationToken cancellationToken)
    {
        var email = ResetTokenHelper.ValidarToken(request.Token, _authSettings.JwtSigningKey);
        if (email is null) return false;

        var usuario = await _usuarioPerfilRepository.ObterPorEmailAsync(email, cancellationToken);
        if (usuario is null || !usuario.Ativo) return false;

        usuario.DefinirSenhaHash(_passwordHasher.HashPassword(usuario, request.NovaSenha));
        await _usuarioPerfilRepository.AtualizarAsync(usuario, cancellationToken);

        return true;
    }
}
