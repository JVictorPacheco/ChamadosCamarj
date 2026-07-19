namespace ChamadosCamarj.Application.Common;

public class AuthSettings
{
    public string GoogleClientId { get; set; } = string.Empty;
    public string JwtSigningKey { get; set; } = string.Empty;
    public int TokenExpiracaoHoras { get; set; } = 10;
}
