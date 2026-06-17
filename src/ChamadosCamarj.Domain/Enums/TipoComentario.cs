using System.Text.Json.Serialization;

namespace ChamadosCamarj.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoComentario
{
    Publico,
    Interno
}
