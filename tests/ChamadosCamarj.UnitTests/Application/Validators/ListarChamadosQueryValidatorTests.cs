using ChamadosCamarj.Application.Features.Chamados.Queries;
using ChamadosCamarj.Application.Features.Chamados.Validators;
using FluentValidation.TestHelper;

namespace ChamadosCamarj.UnitTests.Application.Validators;

public class ListarChamadosQueryValidatorTests
{
    private readonly ListarChamadosQueryValidator _validator = new();

    [Fact]
    public void Validate_ComValoresPadrao_DevePassar()
    {
        var query = new ListarChamadosQuery();

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ComPaginaMenorOuIgualAZero_DeveFalharValidacao(int pagina)
    {
        var query = new ListarChamadosQuery(Pagina: pagina);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.Pagina);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(10000)]
    public void Validate_ComTamanhoPaginaForaDoIntervalo_DeveFalharValidacao(int tamanhoPagina)
    {
        var query = new ListarChamadosQuery(TamanhoPagina: tamanhoPagina);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.TamanhoPagina);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_ComTamanhoPaginaDentroDoIntervalo_DevePassar(int tamanhoPagina)
    {
        var query = new ListarChamadosQuery(TamanhoPagina: tamanhoPagina);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(q => q.TamanhoPagina);
    }

    [Fact]
    public void Validate_ComDataFimAnteriorADataInicio_DeveFalharValidacao()
    {
        var query = new ListarChamadosQuery(DataInicio: new DateTime(2026, 7, 31), DataFim: new DateTime(2026, 7, 1));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.DataFim);
    }

    [Fact]
    public void Validate_ComDataFimIgualOuPosteriorADataInicio_DevePassar()
    {
        var query = new ListarChamadosQuery(DataInicio: new DateTime(2026, 7, 1), DataFim: new DateTime(2026, 7, 31));

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(q => q.DataFim);
    }
}
