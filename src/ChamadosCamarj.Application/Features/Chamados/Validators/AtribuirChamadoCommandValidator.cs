using FluentValidation;
using ChamadosCamarj.Application.Features.Chamados.Commands;

namespace ChamadosCamarj.Application.Features.Chamados.Validators;

public class AtribuirChamadoCommandValidator : AbstractValidator<AtribuirChamadoCommand>
{
    public AtribuirChamadoCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("ID do chamado é obrigatório.");

        RuleFor(c => c.ResponsavelId)
            .NotEmpty().WithMessage("ID do responsável é obrigatório.");

        RuleFor(c => c.ResponsavelNome)
            .NotEmpty().WithMessage("Nome do responsável é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");
    }
}
