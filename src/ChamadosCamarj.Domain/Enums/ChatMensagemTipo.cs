using System.Text.Json.Serialization;

namespace ChamadosCamarj.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatMensagemTipo
{
    Texto = 0,
    Arquivo = 1,
    Sistema = 2
}
