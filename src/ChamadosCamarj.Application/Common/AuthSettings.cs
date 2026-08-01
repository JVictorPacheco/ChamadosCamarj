namespace ChamadosCamarj.Application.Common;

public class AuthSettings
{
    public string GoogleClientId { get; set; } = string.Empty;
    public string JwtSigningKey { get; set; } = string.Empty;
    public string? ResetTokenSigningKey { get; set; }
    public int TokenExpiracaoHoras { get; set; } = 10;
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
}
