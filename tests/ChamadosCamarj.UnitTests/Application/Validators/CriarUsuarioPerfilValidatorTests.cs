using ChamadosCamarj.Application.Features.Usuarios.Commands;
using ChamadosCamarj.Application.Features.Usuarios.Validators;
using ChamadosCamarj.Domain.Enums;
using FluentAssertions;

namespace ChamadosCamarj.UnitTests.Application.Validators;

public class CriarUsuarioPerfilValidatorTests
{
    private readonly CriarUsuarioPerfilCommandValidator _validator = new();

    private static CriarUsuarioPerfilCommand ComandoValido() => new(
        "vitor@camarj.com.br",
        "Vitor",
        Perfil.Admin);

    [Fact]
    public void Validar_ComDadosValidos_DevePassar()
    {
        var result = _validator.Validate(ComandoValido());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-email")]
    [InlineData("semdominio@")]
    public void Validar_ComEmailInvalido_DeveFalhar(string email)
    {
        var command = ComandoValido() with { Email = email };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_ComNomeVazio_DeveFalhar(string nome)
    {
        var command = ComandoValido() with { Nome = nome };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Nome));
    }

    [Fact]
    public void Validar_ComPerfilInvalido_DeveFalhar()
    {
        var command = ComandoValido() with { Perfil = (Perfil)999 };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Perfil));
    }
}
