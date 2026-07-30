using FluentValidation;
using ChamadosCamarj.Application.Features.Categorias.Commands;

namespace ChamadosCamarj.Application.Features.Categorias.Validators;

public class AtualizarCategoriaCommandValidator : AbstractValidator<AtualizarCategoriaCommand>
{
    public AtualizarCategoriaCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("ID da categoria é obrigatório.");

        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");
    }
}
