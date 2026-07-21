using FluentValidation;
using ChamadosCamarj.Application.Features.Chamados.Commands;

namespace ChamadosCamarj.Application.Features.Chamados.Validators;

public class ForcarEncerramentoChamadoCommandValidator : AbstractValidator<ForcarEncerramentoChamadoCommand>
{
    public ForcarEncerramentoChamadoCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("ID do chamado é obrigatório.");

        RuleFor(c => c.Motivo)
            .NotEmpty().WithMessage("Motivo é obrigatório.")
            .Must(motivo => motivo.Trim().Length >= 10).WithMessage("Motivo deve ter no mínimo 10 caracteres.")
            .MaximumLength(500).WithMessage("Motivo deve ter no máximo 500 caracteres.");
    }
}
