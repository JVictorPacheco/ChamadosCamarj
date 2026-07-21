using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Auth.DTOs;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Auth.Commands;

public class AutenticarGoogleCommandHandler : IRequestHandler<AutenticarGoogleCommand, AutenticacaoResponse>
{
    private const string DominioPermitido = "@camarj.com.br";

    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly AuthSettings _authSettings;

    public AutenticarGoogleCommandHandler(
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IGoogleTokenValidator googleTokenValidator,
        IOptions<AuthSettings> authSettings)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _googleTokenValidator = googleTokenValidator;
        _authSettings = authSettings.Value;
    }

    public async Task<AutenticacaoResponse> Handle(AutenticarGoogleCommand request, CancellationToken cancellationToken)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await _googleTokenValidator.ValidateAsync(request.IdToken, _authSettings.GoogleClientId);
        }
        catch (InvalidJwtException)
        {
            throw new UnauthorizedException("Token do Google inválido ou expirado.");
        }

        var emailNormalizado = payload.Email.Trim().ToLowerInvariant();

        // Defesa em profundidade: mesmo com o Google Cloud Console restrito à organização
        // ("Internal") e o frontend usando hosted_domain, o backend confere de novo — nunca
        // confiar só numa restrição do lado do cliente.
        if (!payload.EmailVerified || !emailNormalizado.EndsWith(DominioPermitido, StringComparison.Ordinal))
            throw new UnauthorizedException("Conta Google não pertence ao domínio camarj.com.br.");

        var usuario = await _usuarioPerfilRepository.ObterPorEmailAsync(emailNormalizado, cancellationToken);

        if (usuario is null || !usuario.Ativo)
            throw new ForbiddenException("E-mail não cadastrado — peça a um Admin para te cadastrar.");

        var token = GerarToken(usuario);

        return new AutenticacaoResponse(token, usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil);
    }

    private string GerarToken(UsuarioPerfil usuario)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authSettings.JwtSigningKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim("perfil", usuario.Perfil.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: "ChamadosCamarj",
            audience: "ChamadosCamarj.Frontend",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_authSettings.TokenExpiracaoHoras),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
