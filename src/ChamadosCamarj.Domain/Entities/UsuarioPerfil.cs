using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Domain.Entities;

public class UsuarioPerfil : BaseEntity
{
    private UsuarioPerfil() { }

    public UsuarioPerfil(string email, string nome, Perfil perfil)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("E-mail é obrigatório.", nameof(email));
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));

        Email = email.Trim().ToLowerInvariant();
        Nome = nome;
        Perfil = perfil;
        Ativo = true;
    }

    public string Email { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public Perfil Perfil { get; private set; }
    public bool Ativo { get; private set; }
    public string? SenhaHash { get; private set; }
    public Guid? GrupoId { get; private set; }
    public Grupo? Grupo { get; private set; }

    public void DefinirSenhaHash(string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("Hash de senha é obrigatório.", nameof(senhaHash));

        SenhaHash = senhaHash;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Atualizar(string nome, Perfil perfil, Guid? grupoId = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));

        Nome = nome;
        Perfil = perfil;
        GrupoId = grupoId;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void DefinirGrupo(Guid? grupoId)
    {
        GrupoId = grupoId;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo = false;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Ativar()
    {
        Ativo = true;
        DataAtualizacao = DateTime.UtcNow;
    }
}
