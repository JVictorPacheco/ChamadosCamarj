using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Application.Features.Chamados.Validators;
using FluentAssertions;

namespace ChamadosCamarj.UnitTests.Application.Validators;

public class ForcarEncerramentoChamadoValidatorTests
{
    private readonly ForcarEncerramentoChamadoCommandValidator _validator = new();

    private static ForcarEncerramentoChamadoCommand ComandoValido() => new(
        Guid.NewGuid(),
        "Chamado duplicado, abrindo por engano.");

    [Fact]
    public void Validar_ComDadosValidos_DevePassar()
    {
        var result = _validator.Validate(ComandoValido());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_ComIdVazio_DeveFalhar()
    {
        var command = ComandoValido() with { Id = Guid.Empty };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Id));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_ComMotivoVazio_DeveFalhar(string motivo)
    {
        var command = ComandoValido() with { Motivo = motivo };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Motivo));
    }

    [Fact]
    public void Validar_ComMotivoMenorQue10Chars_DeveFalhar()
    {
        var command = ComandoValido() with { Motivo = "curto" };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Motivo));
    }

    [Fact]
    public void Validar_ComMotivoDe10Chars_DevePassar()
    {
        var command = ComandoValido() with { Motivo = new string('A', 10) };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_ComMotivoCurtoPreenchidoComEspacosPraCompletar10Chars_DeveFalhar()
    {
        // "ok" + 8 espaços tem 10 caracteres "crus", mas o conteúdo real não é uma justificativa —
        // o mínimo precisa ser conferido depois do Trim().
        var command = ComandoValido() with { Motivo = "ok" + new string(' ', 8) };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Motivo));
    }

    [Fact]
    public void Validar_ComMotivoMaiorQue500Chars_DeveFalhar()
    {
        var command = ComandoValido() with { Motivo = new string('A', 501) };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Motivo));
    }
}
