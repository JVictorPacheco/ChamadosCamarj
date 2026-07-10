using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Domain.Entities;

public class HistoricoEntrada : BaseEntity
{
    public Guid ChamadoId { get; private set; }
    public string UsuarioNome { get; private set; } = string.Empty;
    public Guid? UsuarioId { get; private set; }
    public AcaoHistorico Acao { get; private set; }
    public string? DetalheAnterior { get; private set; }
    public string? DetalheNovo { get; private set; }
    public DateTime DataHora { get; private set; }

    // Constructor para EF Core
    public HistoricoEntrada() { }

    // Factory method com validação
    public static HistoricoEntrada Criar(
        Guid chamadoId,
        string usuarioNome,
        Guid? usuarioId,
        AcaoHistorico acao,
        string? detalheAnterior = null,
        string? detalheNovo = null)
    {
        if (chamadoId == Guid.Empty)
            throw new ArgumentException("ChamadoId não pode ser vazio", nameof(chamadoId));

        if (string.IsNullOrWhiteSpace(usuarioNome))
            throw new ArgumentException("UsuarioNome não pode ser vazio", nameof(usuarioNome));

        return new HistoricoEntrada
        {
            ChamadoId = chamadoId,
            UsuarioNome = usuarioNome,
            UsuarioId = usuarioId,
            Acao = acao,
            DetalheAnterior = detalheAnterior,
            DetalheNovo = detalheNovo,
            DataHora = DateTime.UtcNow
        };
    }
}
