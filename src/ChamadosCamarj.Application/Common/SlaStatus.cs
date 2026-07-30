namespace ChamadosCamarj.Application.Common;

public enum SlaStatus
{
    DentroPrazo,
    Atencao,
    Atrasado
}

public static class SlaCalculo
{
    public static SlaStatus CalcularStatus(DateTime? dataLimite)
    {
        if (dataLimite is null) return SlaStatus.DentroPrazo;
        var agora = DateTime.UtcNow;
        if (dataLimite <= agora) return SlaStatus.Atrasado;
        if (dataLimite <= agora.AddHours(2)) return SlaStatus.Atencao;
        return SlaStatus.DentroPrazo;
    }

    public static string FormatarLabel(DateTime? dataLimite)
    {
        if (dataLimite is null) return "";
        var agora = DateTime.UtcNow;
        var diff = dataLimite.Value - agora;
        if (diff.TotalHours <= 0)
        {
            var atraso = -diff;
            return atraso.TotalHours >= 1
                ? $"Atrasado {atraso.Hours}h {atraso.Minutes}min"
                : $"Atrasado {atraso.Minutes}min";
        }
        return diff.TotalHours >= 1
            ? $"Faltam {diff.Hours}h {diff.Minutes}min"
            : $"Fecha em {diff.Minutes}min";
    }

    public static double? CalcularHorasRestantes(DateTime? dataLimite)
    {
        if (dataLimite is null) return null;
        return (dataLimite.Value - DateTime.UtcNow).TotalHours;
    }
}