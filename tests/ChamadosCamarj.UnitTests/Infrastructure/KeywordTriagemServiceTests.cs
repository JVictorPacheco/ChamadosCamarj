using ChamadosCamarj.Infrastructure.Data;
using ChamadosCamarj.Infrastructure.Services;
using FluentAssertions;

namespace ChamadosCamarj.UnitTests.Infrastructure;

/// <summary>spec area-e-tipo-do-chamado AC-04 — a triagem sugere área e tipo.</summary>
public class KeywordTriagemServiceTests
{
    private readonly KeywordTriagemService _service = new();

    [Fact]
    public async Task SugereAreaETipo()
    {
        var s = await _service.SugerirAsync("Erro no reembolso", "O sistema de reembolso não funciona desde ontem");

        s.AreaNome.Should().Be("Reembolso");
        s.TipoId.Should().Be(AreaETipoPadrao.TipoIncidente);
        s.TemSugestao.Should().BeTrue();
    }

    [Theory]
    [InlineData("Como faço para emitir a segunda via?", "Dúvida")]
    [InlineData("Preciso de acesso ao sistema de faturamento", "Solicitação")]
    [InlineData("Sugestão: melhorar o filtro da tela", "Melhoria")]
    [InlineData("Personalizar o layout do relatório", "Customização")]
    public async Task SugereTipo(string titulo, string tipoEsperado)
    {
        var s = await _service.SugerirAsync(titulo, "");

        s.TipoNome.Should().Be(tipoEsperado);
    }

    [Fact]
    public async Task SemPalavraConhecida_NaoSugere()
    {
        var s = await _service.SugerirAsync("Xyz", "abc");

        s.TemSugestao.Should().BeFalse();
    }
}
