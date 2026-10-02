using FluentValidation;
using ChamadosCamarj.Application.Features.Tipos.Commands;

namespace ChamadosCamarj.Application.Features.Tipos.Validators;

public class AtualizarTipoChamadoCommandValidator : AbstractValidator<AtualizarTipoChamadoCommand>
{
    public AtualizarTipoChamadoCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("ID do tipo é obrigatório.");

        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");
    }
}
