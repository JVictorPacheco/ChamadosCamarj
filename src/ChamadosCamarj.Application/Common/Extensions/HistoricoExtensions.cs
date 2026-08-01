using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Common.Extensions;

public static class HistoricoExtensions
{
    public static async Task RegistrarHistoricoAsync(
        this IHistoricoRepository repository,
        Guid chamadoId,
        AcaoHistorico acao,
        string? detalheAnterior = null,
        string? detalheNovo = null,
        string usuarioNome = "Sistema",
        Guid? usuarioId = null,
        OrigemEntrada origem = OrigemEntrada.Humano,
        CancellationToken cancellationToken = default)
    {
        var historico = HistoricoEntrada.Criar(
            chamadoId,
            usuarioNome,
            usuarioId,
            acao,
            detalheAnterior,
            detalheNovo,
            origem
        );

        await repository.AdicionarAsync(historico, cancellationToken);
    }
}
