using System.Text.Json.Serialization;

namespace ChamadosCamarj.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatConversaTipo
{
    Privada = 0,
    Grupo = 1
}
