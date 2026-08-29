using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Domain.Entities;

public class ChatPresenca : BaseEntity
{
    private ChatPresenca() { }

    public ChatPresenca(Guid usuarioId, string usuarioNome)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("UsuarioId não pode ser vazio.", nameof(usuarioId));
        if (string.IsNullOrWhiteSpace(usuarioNome))
            throw new ArgumentException("Nome do usuário é obrigatório.", nameof(usuarioNome));

        UsuarioId = usuarioId;
        UsuarioNome = usuarioNome;
        Status = StatusPresenca.Offline;
        UltimoHeartbeat = DateTime.UtcNow;
    }

    public Guid UsuarioId { get; private set; }
    public string UsuarioNome { get; private set; } = string.Empty;
    public StatusPresenca Status { get; private set; } = StatusPresenca.Offline;
    public DateTime UltimoHeartbeat { get; private set; }

    public void AtualizarHeartbeat()
    {
        Status = StatusPresenca.Online;
        UltimoHeartbeat = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void DefinirStatus(StatusPresenca status)
    {
        Status = status;
        if (status == StatusPresenca.Online)
            UltimoHeartbeat = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void MarcarAusente()
    {
        Status = StatusPresenca.Ausente;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void MarcarOffline()
    {
        Status = StatusPresenca.Offline;
        DataAtualizacao = DateTime.UtcNow;
    }
}
