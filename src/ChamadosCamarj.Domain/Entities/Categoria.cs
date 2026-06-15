using ChamadosCamarj.Domain.Common;

namespace ChamadosCamarj.Domain.Entities;

public class Categoria : BaseEntity
{
    private Categoria() { }

    public Categoria(string nome, string descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));

        Nome = nome;
        Descricao = descricao ?? string.Empty;
        Ativa = true;
    }

    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public bool Ativa { get; private set; }

    // Navegação EF
    public ICollection<Chamado> Chamados { get; private set; } = [];

    public void Desativar()
    {
        Ativa = false;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Ativar()
    {
        Ativa = true;
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
