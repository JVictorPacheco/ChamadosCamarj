using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Application.Features.Chamados.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ChamadosCamarj.UnitTests.Application.Validators;

public class ReatribuirChamadoValidatorTests
{
    private readonly ReatribuirChamadoCommandValidator _validator = new();

    [Fact]
    public void Validate_ComDadosValidos_DevePassar()
    {
        var command = new ReatribuirChamadoCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Victor"
        );

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ComIdVazio_DevefalharValidacao()
    {
        var command = new ReatribuirChamadoCommand(
            Guid.Empty,
            Guid.NewGuid(),
            "Victor"
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Validate_ComNovoResponsavelIdVazio_DevefalharValidacao()
    {
        var command = new ReatribuirChamadoCommand(
            Guid.NewGuid(),
            Guid.Empty,
            "Victor"
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.NovoResponsavelId);
    }

    [Fact]
    public void Validate_ComNomeVazio_DevefalharValidacao()
    {
        var command = new ReatribuirChamadoCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            string.Empty
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.NovoResponsavelNome);
    }

    [Fact]
    public void Validate_ComNomeExcedendoMaximoCaracteres_DevefalharValidacao()
    {
        var command = new ReatribuirChamadoCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new string('A', 151)
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.NovoResponsavelNome);
    }
}

public class AlterarPrioridadeValidatorTests
{
    private readonly AlterarPrioridadeChamadoCommandValidator _validator = new();

    [Theory]
    [InlineData("Urgente")]
    [InlineData("Alta")]
    [InlineData("Media")]
    [InlineData("Baixa")]
    public void Validate_ComPrioridadesValidas_DevemPassar(string prioridade)
    {
        var command = new AlterarPrioridadeChamadoCommand(Guid.NewGuid(), prioridade);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("urgente")]
    [InlineData("URGENTE")]
    [InlineData("alta")]
    public void Validate_ComPrioridadesValidasEmOutrosCasing_DevemPassar(string prioridade)
    {
        var command = new AlterarPrioridadeChamadoCommand(Guid.NewGuid(), prioridade);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ComPrioridadeInvalida_DevefalharValidacao()
    {
        var command = new AlterarPrioridadeChamadoCommand(Guid.NewGuid(), "Criticíssima");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.NovaPrioridade);
    }

    [Fact]
    public void Validate_ComIdVazio_DevefalharValidacao()
    {
        var command = new AlterarPrioridadeChamadoCommand(Guid.Empty, "Urgente");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Validate_ComPrioridadeVazia_DevefalharValidacao()
    {
        var command = new AlterarPrioridadeChamadoCommand(Guid.NewGuid(), string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.NovaPrioridade);
    }
}
