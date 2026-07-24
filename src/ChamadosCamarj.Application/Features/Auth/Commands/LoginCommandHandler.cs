using MediatR;
using Microsoft.AspNetCore.Identity;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Auth.DTOs;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AutenticacaoResponse>
{
    private const string MensagemCredenciaisInvalidas = "E-mail ou senha inválidos.";

    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IPasswordHasher<UsuarioPerfil> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IPasswordHasher<UsuarioPerfil> passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AutenticacaoResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();
        var usuario = await _usuarioPerfilRepository.ObterPorEmailAsync(emailNormalizado, cancellationToken);

        // Mensagem sempre genérica (mesmo com o usuário inativo ou sem senha configurada) —
        // não dá pra diferenciar "e-mail não existe" de "senha errada" pro cliente, evita
        // enumeração de contas cadastradas.
        if (usuario is null || !usuario.Ativo || string.IsNullOrEmpty(usuario.SenhaHash))
            throw new UnauthorizedException(MensagemCredenciaisInvalidas);

        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, request.Senha);
        if (resultado == PasswordVerificationResult.Failed)
            throw new UnauthorizedException(MensagemCredenciaisInvalidas);

        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.DefinirSenhaHash(_passwordHasher.HashPassword(usuario, request.Senha));
            await _usuarioPerfilRepository.AtualizarAsync(usuario, cancellationToken);
        }

        var token = _jwtTokenService.GerarToken(usuario);

        return new AutenticacaoResponse(token, usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil);
    }
}
