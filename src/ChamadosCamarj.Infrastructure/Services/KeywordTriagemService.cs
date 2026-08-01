using ChamadosCamarj.Application.Common.Interfaces;

namespace ChamadosCamarj.Infrastructure.Services;

public class KeywordTriagemService : ITriagemService
{
    private static readonly Dictionary<Guid, (string Nome, string[] Keywords)> Categorias = new()
    {
        [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567891")] = ("Autorização/Auditoria", ["autorização", "autorizacao", "auditoria", "auditar", "aprovação", "aprovacao"]),
        [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567892")] = ("Atendimento", ["atendimento", "suporte", "ajuda", "dúvida", "duvida", "informação", "informacao", "orientação", "orientacao"]),
        [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567893")] = ("Super e Tendência", ["supervisão", "supervisao", "tendência", "tendencia", "superintendência", "superintendencia", "gestão", "gestao"]),
        [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567894")] = ("Reembolso", ["reembolso", "restituição", "restituicao", "devolução", "devolucao", "ressarcimento", "pagamento"]),
        [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567895")] = ("Financeiro", ["financeiro", "fatura", "faturamento", "nota fiscal", "boleto", "cobrança", "cobranca", "pagamento", "conta"]),
        [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567896")] = ("Credenciado", ["credenciado", "credenciamento", "credenciar", "rede credenciada", "prestador"]),
        [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567897")] = ("Comercial", ["comercial", "contrato", "venda", "negociação", "negociacao", "proposta", "cliente"]),
        [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567898")] = ("Contas Médicas", ["conta médica", "conta medica", "médico", "medico", "hospital", "procedimento", "cirurgia", "consulta", "guia", "paciente"]),
    };

    private static readonly Dictionary<Guid, (string Nome, string[] Keywords)> Grupos = new()
    {
        [Guid.Parse("b1000000-0000-0000-0000-000000000001")] = ("Reembolso", ["reembolso", "restituição", "restituicao", "devolução", "devolucao", "ressarcimento"]),
        [Guid.Parse("b1000000-0000-0000-0000-000000000002")] = ("Credenciado", ["credenciado", "credenciamento", "credenciar", "rede credenciada", "prestador"]),
        [Guid.Parse("b1000000-0000-0000-0000-000000000003")] = ("Comercial", ["comercial", "contrato", "venda", "negociação", "negociacao", "proposta"]),
        [Guid.Parse("b1000000-0000-0000-0000-000000000004")] = ("Contas Médicas", ["conta médica", "conta medica", "médico", "medico", "hospital", "procedimento", "cirurgia", "consulta", "guia", "paciente"]),
        [Guid.Parse("b1000000-0000-0000-0000-000000000005")] = ("Autorização/Auditoria", ["autorização", "autorizacao", "auditoria", "auditar", "aprovação", "aprovacao"]),
        [Guid.Parse("b1000000-0000-0000-0000-000000000006")] = ("Atendimento", ["atendimento", "suporte", "ajuda", "dúvida", "duvida", "informação", "informacao"]),
    };

    public Task<TriagemSugestao> SugerirAsync(string titulo, string descricao, CancellationToken cancellationToken = default)
    {
        var texto = $"{titulo ?? ""} {descricao ?? ""}".ToLowerInvariant();

        var (categoriaId, categoriaNome, catScore) = MelhorMatch(texto, Categorias);
        var (grupoId, grupoNome, _) = MelhorMatch(texto, Grupos);

        return Task.FromResult(new TriagemSugestao
        {
            CategoriaId = categoriaId,
            CategoriaNome = categoriaNome,
            GrupoId = grupoId,
            GrupoNome = grupoNome,
            Confianca = catScore
        });
    }

    private static (Guid? Id, string? Nome, int Score) MelhorMatch(string texto, Dictionary<Guid, (string Nome, string[] Keywords)> dicionario)
    {
        Guid? melhorId = null;
        string? melhorNome = null;
        var melhorScore = 0;

        foreach (var (id, (nome, keywords)) in dicionario)
        {
            var score = keywords.Count(k => texto.Contains(k, StringComparison.OrdinalIgnoreCase));
            if (score > melhorScore)
            {
                melhorScore = score;
                melhorId = id;
                melhorNome = nome;
            }
        }

        return (melhorId, melhorNome, melhorScore);
    }
}
