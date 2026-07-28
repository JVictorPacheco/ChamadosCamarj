using ChamadosCamarj.Domain.Common;

namespace ChamadosCamarj.Domain.Entities;

public class Grupo : BaseEntity
{
    private Grupo() { }

    public Grupo(string nome, string descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));

        Nome = nome;
        Descricao = descricao ?? string.Empty;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }

    public ICollection<UsuarioPerfil> Usuarios { get; private set; } = [];

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

    public void Atualizar(string nome, string descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));

        Nome = nome;
        Descricao = descricao ?? string.Empty;
        DataAtualizacao = DateTime.UtcNow;
    }
}
