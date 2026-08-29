using System.Text.Json.Serialization;

namespace ChamadosCamarj.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusPresenca
{
    Online = 0,
    Ausente = 1,
    Offline = 2
}
