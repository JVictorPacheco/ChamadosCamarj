namespace ChamadosCamarj.Application.Common;

/// <summary>
/// Versão de um chamado para a checagem de edição simultânea (spec correcoes-pre-deploy AC-09):
/// a última alteração, ou a criação se nunca foi alterado, em MICROSSEGUNDOS — a precisão do
/// Postgres. O Npgsql trunca os Ticks ao gravar, então o valor recém-calculado em memória e o relido
/// do banco dão a mesma versão. O cliente trata como texto opaco (nunca converter para Date do JS).
/// </summary>
public static class VersaoChamado
{
    public const string MensagemConflito =
        "Outra pessoa alterou este chamado. Os dados foram atualizados; confira e refaça a ação.";

    public static string De(DateTime? dataAtualizacao, DateTime dataCriacao) =>
        ((dataAtualizacao ?? dataCriacao).Ticks / 10).ToString(System.Globalization.CultureInfo.InvariantCulture);
}
