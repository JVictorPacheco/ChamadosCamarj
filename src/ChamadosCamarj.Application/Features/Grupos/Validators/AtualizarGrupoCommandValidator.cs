using FluentValidation;
using ChamadosCamarj.Application.Features.Grupos.Commands;

namespace ChamadosCamarj.Application.Features.Grupos.Validators;

public class AtualizarGrupoCommandValidator : AbstractValidator<AtualizarGrupoCommand>
{
    public AtualizarGrupoCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("ID do grupo é obrigatório.");

        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");
    }
}
