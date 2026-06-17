using System.Text.Json.Serialization;

namespace ChamadosCamarj.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrigemChamado
{
    Email,
    Portal,
    Api
}
