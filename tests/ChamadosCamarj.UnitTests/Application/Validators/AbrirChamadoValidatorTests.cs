using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Application.Features.Chamados.Validators;
using FluentAssertions;

namespace ChamadosCamarj.UnitTests.Application.Validators;

public class AbrirChamadoValidatorTests
{
    private readonly AbrirChamadoCommandValidator _validator = new();

    private static AbrirChamadoCommand ComandoValido() => new(
        "Título válido",
        "Descrição válida",
        "João Victor",
        "joao@camarj.com.br",
        Guid.NewGuid());

    [Fact]
    public void Validar_ComDadosValidos_DevePassar()
    {
        var result = _validator.Validate(ComandoValido());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_ComTituloVazio_DeveFalhar(string titulo)
    {
        var command = ComandoValido() with { Titulo = titulo };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Titulo));
    }

    [Fact]
    public void Validar_ComTituloMaiorQue200Chars_DeveFalhar()
    {
        var command = ComandoValido() with { Titulo = new string('A', 201) };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_ComDescricaoVazia_DeveFalhar(string descricao)
    {
        var command = ComandoValido() with { Descricao = descricao };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Descricao));
    }

    [Theory]
    [InlineData("nao-e-email")]
    [InlineData("semdominio@")]
    [InlineData("")]
    public void Validar_ComEmailInvalido_DeveFalhar(string email)
    {
        var command = ComandoValido() with { SolicitanteEmail = email };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.SolicitanteEmail));
    }

    [Fact]
    public void Validar_ComCategoriaIdVazio_DeveFalhar()
    {
        var command = ComandoValido() with { CategoriaId = Guid.Empty };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.CategoriaId));
    }
}
