using FluentValidation;
using ChamadosCamarj.Application.Features.Grupos.Commands;

namespace ChamadosCamarj.Application.Features.Grupos.Validators;

public class CriarGrupoCommandValidator : AbstractValidator<CriarGrupoCommand>
{
    public CriarGrupoCommandValidator()
    {
        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");
    }
}
