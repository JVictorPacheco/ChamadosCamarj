using System.Text.Json.Serialization;

namespace ChamadosCamarj.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AcaoHistorico
{
    Criado = 1,
    Assumido = 2,
    Reatribuido = 3,
    Resolvido = 4,
    Fechado = 5,
    Cancelado = 6,
    ComentarioAdicionado = 7,
    PrioridadeAlterada = 8,
    StatusAlterado = 9,
    EncerramentoForcado = 10
}
