using FluentValidation;
using ChamadosCamarj.Application.Features.Categorias.Commands;

namespace ChamadosCamarj.Application.Features.Categorias.Validators;

public class CriarCategoriaCommandValidator : AbstractValidator<CriarCategoriaCommand>
{
    public CriarCategoriaCommandValidator()
    {
        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(c => c.Descricao)
            .MaximumLength(300).WithMessage("Descrição deve ter no máximo 300 caracteres.");
    }
}
