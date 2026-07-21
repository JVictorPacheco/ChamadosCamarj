using FluentValidation;
using ChamadosCamarj.Application.Features.Chamados.Commands;

namespace ChamadosCamarj.Application.Features.Chamados.Validators;

public class AdicionarAnexoCommandValidator : AbstractValidator<AdicionarAnexoCommand>
{
    private const long TamanhoMaximoBytes = 10 * 1024 * 1024; // 10MB

    private static readonly HashSet<string> ExtensoesPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".doc", ".docx", ".xls", ".xlsx", ".zip"
    };

    public AdicionarAnexoCommandValidator()
    {
        RuleFor(c => c.ChamadoId)
            .NotEmpty().WithMessage("ID do chamado é obrigatório.");

        RuleFor(c => c.NomeArquivoOriginal)
            .NotEmpty().WithMessage("Nome do arquivo é obrigatório.")
            .Must(nome => ExtensoesPermitidas.Contains(Path.GetExtension(nome)))
            .WithMessage("Tipo de arquivo não permitido. Tipos aceitos: PDF, imagens (jpg/png/gif), Word, Excel, ZIP.");

        RuleFor(c => c.TamanhoBytes)
            .GreaterThan(0).WithMessage("Arquivo vazio não pode ser enviado.")
            .LessThanOrEqualTo(TamanhoMaximoBytes).WithMessage("Arquivo excede o tamanho máximo de 10MB.");
    }
}
