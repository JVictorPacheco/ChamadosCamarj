using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Application.Features.Chamados.Validators;
using ChamadosCamarj.Domain.Enums;
using FluentAssertions;

namespace ChamadosCamarj.UnitTests.Application.Validators;

public class ForcarEncerramentoChamadoValidatorTests
{
    private readonly ForcarEncerramentoChamadoCommandValidator _validator = new();

    private static ForcarEncerramentoChamadoCommand ComandoValido() => new(
        Guid.NewGuid(),
        MotivoEncerramento.AbertoIndevidamente);

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

    [Fact]
    public void Validar_ComMotivoOutro_QuandoMotivoOutroVazio_DeveFalhar()
    {
        var command = ComandoValido() with { Motivo = MotivoEncerramento.Outro, MotivoOutro = "" };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.MotivoOutro));
    }

    [Fact]
    public void Validar_ComMotivoOutro_QuandoMotivoOutroPreenchido_DevePassar()
    {
        var command = ComandoValido() with { Motivo = MotivoEncerramento.Outro, MotivoOutro = "Chamado criado por engano." };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_ComMotivoOutro_QuandoMotivoNaoEOutro_DevePassar()
    {
        var command = ComandoValido() with { Motivo = MotivoEncerramento.Duplicata, MotivoOutro = null };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
