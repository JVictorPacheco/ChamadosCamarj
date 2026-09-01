using FluentValidation;

namespace ChamadosCamarj.Application.Features.Chat.Commands.EnviarArquivo;

public class EnviarArquivoCommandValidator : AbstractValidator<EnviarArquivoCommand>
{
    private const long TamanhoMaximoBytes = 10 * 1024 * 1024; // 10MB

    private static readonly HashSet<string> ExtensoesPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".zip"
    };

    public EnviarArquivoCommandValidator()
    {
        RuleFor(c => c.ConversaId)
            .NotEmpty().WithMessage("ID da conversa é obrigatório.");

        RuleFor(c => c.NomeArquivoOriginal)
            .NotEmpty().WithMessage("Nome do arquivo é obrigatório.")
            .Must(nome => ExtensoesPermitidas.Contains(Path.GetExtension(nome)))
            .WithMessage("Tipo de arquivo não permitido. Tipos aceitos: PDF, imagens (JPG/PNG/GIF/WebP), Office (DOCX/XLSX/PPTX) e ZIP.");

        RuleFor(c => c.TamanhoBytes)
            .GreaterThan(0).WithMessage("Arquivo vazio não pode ser enviado.")
            .LessThanOrEqualTo(TamanhoMaximoBytes).WithMessage("Arquivo excede o tamanho máximo de 10MB.");
    }
}
