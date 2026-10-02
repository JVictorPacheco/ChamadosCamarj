using ChamadosCamarj.Application.Common.Interfaces;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Services;

/// <summary>
/// Sugere área (grupo) e tipo do chamado por palavras-chave no título/descrição
/// (spec area-e-tipo-do-chamado AC-04). A lista de palavras é uma proposta inicial, ajustável.
/// </summary>
public class KeywordTriagemService : ITriagemService
{
    private static readonly Dictionary<Guid, (string Nome, string[] Keywords)> Areas = new()
    {
        [Guid.Parse("b1000000-0000-0000-0000-000000000001")] = ("Reembolso", ["reembolso", "restituição", "restituicao", "devolução", "devolucao", "ressarcimento"]),
        [Guid.Parse("b1000000-0000-0000-0000-000000000002")] = ("Credenciado", ["credenciado", "credenciamento", "credenciar", "rede credenciada", "prestador"]),
        [Guid.Parse("b1000000-0000-0000-0000-000000000003")] = ("Comercial", ["comercial", "contrato", "venda", "negociação", "negociacao", "proposta", "cliente"]),
        [Guid.Parse("b1000000-0000-0000-0000-000000000004")] = ("Contas Médicas", ["conta médica", "conta medica", "médico", "medico", "hospital", "procedimento", "cirurgia", "consulta", "guia", "paciente"]),
        [Guid.Parse("b1000000-0000-0000-0000-000000000005")] = ("Autorização/Auditoria", ["autorização", "autorizacao", "auditoria", "auditar", "aprovação", "aprovacao"]),
        [Guid.Parse("b1000000-0000-0000-0000-000000000006")] = ("Atendimento", ["atendimento", "ligação", "ligacao", "telefone", "beneficiário", "beneficiario"]),
        [AreaETipoPadrao.AreaFinanceiro] = ("Financeiro", ["financeiro", "fatura", "faturamento", "nota fiscal", "boleto", "cobrança", "cobranca", "pagamento"]),
        [AreaETipoPadrao.AreaSuperETendencia] = ("Super e Tendência", ["supervisão", "supervisao", "tendência", "tendencia", "superintendência", "superintendencia", "gestão", "gestao"]),
    };

    private static readonly Dictionary<Guid, (string Nome, string[] Keywords)> Tipos = new()
    {
        [AreaETipoPadrao.TipoIncidente] = ("Incidente", ["erro", "não funciona", "nao funciona", "parou", "travou", "travando", "falha", "caiu", "fora do ar", "bug", "não abre", "nao abre", "lento"]),
        [AreaETipoPadrao.TipoDuvida] = ("Dúvida", ["dúvida", "duvida", "como faço", "como faco", "como fazer", "onde fica", "como funciona", "?"]),
        [AreaETipoPadrao.TipoSolicitacao] = ("Solicitação", ["solicito", "solicitação", "solicitacao", "preciso de", "acesso", "cadastrar", "cadastro", "liberar", "segunda via", "instalar"]),
        [AreaETipoPadrao.TipoCustomizacao] = ("Customização", ["customizar", "customização", "customizacao", "personalizar", "personalização", "personalizacao", "ajustar o", "alterar o layout"]),
        [AreaETipoPadrao.TipoMelhoria] = ("Melhoria", ["melhoria", "melhorar", "sugestão", "sugestao", "seria bom", "poderia ter", "otimizar"]),
    };

    public Task<TriagemSugestao> SugerirAsync(string titulo, string descricao, CancellationToken cancellationToken = default)
    {
        var texto = $"{titulo ?? ""} {descricao ?? ""}".ToLowerInvariant();

        var (areaId, areaNome, areaScore) = MelhorMatch(texto, Areas);
        var (tipoId, tipoNome, tipoScore) = MelhorMatch(texto, Tipos);

        return Task.FromResult(new TriagemSugestao
        {
            AreaId = areaId,
            AreaNome = areaNome,
            TipoId = tipoId,
            TipoNome = tipoNome,
            Confianca = areaScore + tipoScore
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
