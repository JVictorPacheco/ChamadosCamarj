using ChamadosCamarj.Application.Common.Interfaces;

namespace ChamadosCamarj.WebApi.Services;

/// <summary>Lê a versão do chamado do cabeçalho If-Match (com ou sem aspas). Spec correcoes-pre-deploy.</summary>
public class VersaoLidaAccessor : IVersaoLidaAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VersaoLidaAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? VersaoLida => Normalizar(_httpContextAccessor.HttpContext?.Request.Headers.IfMatch.ToString());

    public static string? Normalizar(string? valor)
    {
        var limpo = valor?.Trim().Trim('"').Trim();
        return string.IsNullOrEmpty(limpo) ? null : limpo;
    }
}
