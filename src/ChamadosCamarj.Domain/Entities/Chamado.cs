using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Common;

namespace ChamadosCamarj.Domain.Entities;

public class Chamado : BaseEntity
{
    private Chamado() { }

    public Chamado(
        string titulo,
        string descricao,
        string solicitanteNome,
        string solicitanteEmail,
        Guid categoriaId,
        PrioridadeChamado prioridade = PrioridadeChamado.Media,
        OrigemChamado origem = OrigemChamado.Portal)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ArgumentException("Título é obrigatório.", nameof(titulo));
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição é obrigatória.", nameof(descricao));
        if (string.IsNullOrWhiteSpace(solicitanteNome))
            throw new ArgumentException("Nome do solicitante é obrigatório.", nameof(solicitanteNome));
        if (string.IsNullOrWhiteSpace(solicitanteEmail))
            throw new ArgumentException("Email do solicitante é obrigatório.", nameof(solicitanteEmail));

        Titulo = titulo;
        Descricao = descricao;
        SolicitanteNome = solicitanteNome;
        SolicitanteEmail = solicitanteEmail;
        CategoriaId = categoriaId;
        Prioridade = prioridade;
        Origem = origem;
        Status = StatusChamado.Aberto;
        DataLimite = CalcularDataLimite(prioridade);
    }

    // Propriedades
    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public StatusChamado Status { get; private set; }
    public PrioridadeChamado Prioridade { get; private set; }
    public string SolicitanteNome { get; private set; } = string.Empty;
    public string SolicitanteEmail { get; private set; } = string.Empty;
    public Guid? ResponsavelId { get; private set; }
    public string? ResponsavelNome { get; private set; }
    public Guid CategoriaId { get; private set; }
    public DateTime? DataLimite { get; private set; }
    public DateTime? DataConclusao { get; private set; }
    public OrigemChamado Origem { get; private set; }

    // Navegação EF
    public Categoria? Categoria { get; private set; }
    public ICollection<Comentario> Comentarios { get; private set; } = [];
    public ICollection<Anexo> Anexos { get; private set; } = [];

    // Métodos de negócio
    public void Atribuir(Guid responsavelId, string responsavelNome)
    {
        if (Status is StatusChamado.Fechado or StatusChamado.Cancelado)
            throw new InvalidOperationException($"Não é possível atribuir um chamado com status '{Status}'.");

        ResponsavelId = responsavelId;
        ResponsavelNome = responsavelNome;
        Status = StatusChamado.EmAndamento;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Resolver()
    {
        if (Status is not (StatusChamado.Aberto or StatusChamado.EmAndamento))
            throw new InvalidOperationException($"Não é possível resolver um chamado com status '{Status}'.");

        Status = StatusChamado.Resolvido;
        DataConclusao = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Fechar()
    {
        if (Status != StatusChamado.Resolvido)
            throw new InvalidOperationException("Só é possível fechar um chamado que já foi resolvido.");

        Status = StatusChamado.Fechado;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Reabrir()
    {
        if (Status is not (StatusChamado.Resolvido or StatusChamado.Fechado or StatusChamado.Cancelado))
            throw new InvalidOperationException($"Não é possível reabrir um chamado com status '{Status}'.");

        Status = StatusChamado.Aberto;
        ResponsavelId = null;
        ResponsavelNome = null;
        DataConclusao = null;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Cancelar()
    {
        if (Status is StatusChamado.Fechado or StatusChamado.Cancelado)
            throw new InvalidOperationException($"Não é possível cancelar um chamado com status '{Status}'.");

        Status = StatusChamado.Cancelado;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AlterarPrioridade(PrioridadeChamado novaPrioridade)
    {
        if (Status is StatusChamado.Fechado or StatusChamado.Cancelado)
            throw new InvalidOperationException($"Não é possível alterar a prioridade de um chamado com status '{Status}'.");

        Prioridade = novaPrioridade;
        DataLimite = CalcularDataLimite(novaPrioridade);
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AtualizarDados(string titulo, string descricao)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ArgumentException("Título é obrigatório.", nameof(titulo));
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição é obrigatória.", nameof(descricao));

        Titulo = titulo;
        Descricao = descricao;
        DataAtualizacao = DateTime.UtcNow;
    }

    private static DateTime? CalcularDataLimite(PrioridadeChamado prioridade) => prioridade switch
    {
        PrioridadeChamado.Urgente => DateTime.UtcNow.AddHours(8),
        PrioridadeChamado.Alta => DateTime.UtcNow.AddHours(24),
        PrioridadeChamado.Media => DateTime.UtcNow.AddHours(16),
        PrioridadeChamado.Baixa => DateTime.UtcNow.AddHours(48),
        _ => null
    };
}
