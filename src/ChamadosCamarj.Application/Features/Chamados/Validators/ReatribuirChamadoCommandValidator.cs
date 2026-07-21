using FluentValidation;
using ChamadosCamarj.Application.Features.Chamados.Commands;

namespace ChamadosCamarj.Application.Features.Chamados.Validators;

public class ReatribuirChamadoCommandValidator : AbstractValidator<ReatribuirChamadoCommand>
{
    public ReatribuirChamadoCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("ID do chamado é obrigatório.");

        RuleFor(c => c.NovoResponsavelId)
            .NotEmpty().WithMessage("ID do novo responsável é obrigatório.");

        RuleFor(c => c.NovoResponsavelNome)
            .NotEmpty().WithMessage("Nome do novo responsável é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");
    }
}
