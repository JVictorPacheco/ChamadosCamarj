using MediatR;
using Microsoft.Extensions.Options;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Auth.Commands;

public class EsqueciSenhaCommandHandler : IRequestHandler<EsqueciSenhaCommand>
{
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IEmailSender _emailSender;
    private readonly AuthSettings _authSettings;

    public EsqueciSenhaCommandHandler(
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IEmailSender emailSender,
        IOptions<AuthSettings> authSettings)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _emailSender = emailSender;
        _authSettings = authSettings.Value;
    }

    public async Task Handle(EsqueciSenhaCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioPerfilRepository.ObterPorEmailAsync(request.Email, cancellationToken);

        if (usuario is null || !usuario.Ativo) return;

        var token = ResetTokenHelper.GerarToken(usuario.Email, _authSettings.JwtSigningKey, TimeSpan.FromHours(1));
        var link = $"{_authSettings.FrontendBaseUrl}/resetar-senha?token={Uri.EscapeDataString(token)}";

        var html = $"""
            <div style="font-family: sans-serif; max-width: 480px; margin: 0 auto;">
                <h2>Redefinir senha — Chamados CAMARJ</h2>
                <p>Você solicitou a redefinição da sua senha.</p>
                <p>
                    <a href="{link}" style="display: inline-block; background: #1a1a1a; color: #fff; padding: 10px 20px; text-decoration: none; border-radius: 4px;">
                        Redefinir senha
                    </a>
                </p>
                <p style="color: #666; font-size: 12px;">Este link expira em 1 hora. Se você não solicitou esta redefinição, ignore este email.</p>
            </div>
            """;

        await _emailSender.EnviarAsync(usuario.Email, "Redefinição de senha — Chamados CAMARJ", html);
    }
}
