using Google.Apis.Auth;
using MediatR;
using Microsoft.Extensions.Options;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Auth.DTOs;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Auth.Commands;

public class AutenticarGoogleCommandHandler : IRequestHandler<AutenticarGoogleCommand, AutenticacaoResponse>
{
    private const string DominioPermitido = "@camarj.com.br";

    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly AuthSettings _authSettings;

    public AutenticarGoogleCommandHandler(
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IGoogleTokenValidator googleTokenValidator,
        IJwtTokenService jwtTokenService,
        IOptions<AuthSettings> authSettings)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _googleTokenValidator = googleTokenValidator;
        _jwtTokenService = jwtTokenService;
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

        var token = _jwtTokenService.GerarToken(usuario);

        return new AutenticacaoResponse(token, usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil, usuario.ChatPerfil);
    }
}
