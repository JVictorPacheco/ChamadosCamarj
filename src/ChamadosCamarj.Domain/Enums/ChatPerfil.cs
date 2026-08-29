using System.Text.Json.Serialization;

namespace ChamadosCamarj.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatPerfil
{
    SemAcesso = 0,
    Participante = 1,
    CriadorDeGrupo = 2
}
