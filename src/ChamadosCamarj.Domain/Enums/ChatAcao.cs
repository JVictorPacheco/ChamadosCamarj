using System.Text.Json.Serialization;

namespace ChamadosCamarj.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatAcao
{
    AcessoConcedido,
    AcessoRevogado,
    MensagemEnviada,
    MensagemEditada,
    MensagemDeletada,
    ArquivoEnviado,
    GrupoCriado,
    GrupoDeletado,
    ParticipanteAdicionado,
    ParticipanteRemovido,
    ReacaoAdicionada,
    ReacaoRemovida
}
