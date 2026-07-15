using ChamadosCamarj.Application.Features.Relatorios.Queries;
using ChamadosCamarj.Application.Features.Relatorios.Validators;
using FluentValidation.TestHelper;

namespace ChamadosCamarj.UnitTests.Application.Relatorios;

public class ObterRelatorioMensalQueryValidatorTests
{
    private readonly ObterRelatorioMensalQueryValidator _validator = new();

    [Fact]
    public void Validate_ComDadosValidos_DevePassar()
    {
        var query = new ObterRelatorioMensalQuery(2026, 7);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void Validate_ComMesForaDoIntervalo_DeveFalharValidacao(int mes)
    {
        var query = new ObterRelatorioMensalQuery(2026, mes);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.Mes);
    }

    [Fact]
    public void Validate_ComAnoMuitoAntigo_DeveFalharValidacao()
    {
        var query = new ObterRelatorioMensalQuery(2019, 7);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.Ano);
    }

    [Fact]
    public void Validate_ComAnoMuitoNoFuturo_DeveFalharValidacao()
    {
        var query = new ObterRelatorioMensalQuery(DateTime.UtcNow.Year + 5, 7);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.Ano);
    }
}
