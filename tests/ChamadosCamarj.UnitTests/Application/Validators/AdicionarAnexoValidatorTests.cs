using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Application.Features.Chamados.Validators;
using FluentAssertions;

namespace ChamadosCamarj.UnitTests.Application.Validators;

public class AdicionarAnexoValidatorTests
{
    private readonly AdicionarAnexoCommandValidator _validator = new();

    private static AdicionarAnexoCommand ComandoValido() => new(
        Guid.NewGuid(),
        null,
        "nota-fiscal.pdf",
        "application/pdf",
        Stream.Null,
        1024);

    [Fact]
    public void Validar_ComDadosValidos_DevePassar()
    {
        var result = _validator.Validate(ComandoValido());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_ComChamadoIdVazio_DeveFalhar()
    {
        var command = ComandoValido() with { ChamadoId = Guid.Empty };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ChamadoId));
    }

    [Theory]
    [InlineData("virus.exe")]
    [InlineData("script.js")]
    [InlineData("arquivo-sem-extensao")]
    public void Validar_ComExtensaoNaoPermitida_DeveFalhar(string nomeArquivo)
    {
        var command = ComandoValido() with { NomeArquivoOriginal = nomeArquivo };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.NomeArquivoOriginal));
    }

    [Theory]
    [InlineData("foto.jpg")]
    [InlineData("foto.JPEG")]
    [InlineData("planilha.xlsx")]
    [InlineData("documento.docx")]
    [InlineData("arquivos.zip")]
    public void Validar_ComExtensaoPermitida_DevePassar(string nomeArquivo)
    {
        var command = ComandoValido() with { NomeArquivoOriginal = nomeArquivo };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_ComArquivoVazio_DeveFalhar()
    {
        var command = ComandoValido() with { TamanhoBytes = 0 };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.TamanhoBytes));
    }

    [Fact]
    public void Validar_ComArquivoMaiorQue10MB_DeveFalhar()
    {
        var command = ComandoValido() with { TamanhoBytes = 10 * 1024 * 1024 + 1 };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.TamanhoBytes));
    }

    [Fact]
    public void Validar_ComArquivoDeExatos10MB_DevePassar()
    {
        var command = ComandoValido() with { TamanhoBytes = 10 * 1024 * 1024 };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
