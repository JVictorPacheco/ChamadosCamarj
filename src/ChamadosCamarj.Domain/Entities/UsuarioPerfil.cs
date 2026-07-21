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

    public void Atualizar(string nome, Perfil perfil)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));

        Nome = nome;
        Perfil = perfil;
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
