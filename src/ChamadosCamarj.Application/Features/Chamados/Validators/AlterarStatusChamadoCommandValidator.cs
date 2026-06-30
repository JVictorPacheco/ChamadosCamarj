using FluentValidation;
using ChamadosCamarj.Application.Features.Chamados.Commands;

namespace ChamadosCamarj.Application.Features.Chamados.Validators;

public class AlterarStatusChamadoCommandValidator : AbstractValidator<AlterarStatusChamadoCommand>
{
    public AlterarStatusChamadoCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("ID do chamado é obrigatório.");

        RuleFor(c => c.NovoStatus)
            .IsInEnum().WithMessage("Status inválido.");
    }
}
