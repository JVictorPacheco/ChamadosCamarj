using System.Text.Json.Serialization;

namespace ChamadosCamarj.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Perfil
{
    Admin = 1,
    Atendente = 2,
    Solicitante = 3
}
