using FluentValidation;
using ChamadosCamarj.Application.Features.Chamados.Commands;

namespace ChamadosCamarj.Application.Features.Chamados.Validators;

public class AtualizarChamadoCommandValidator : AbstractValidator<AtualizarChamadoCommand>
{
    public AtualizarChamadoCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("ID do chamado é obrigatório.");

        RuleFor(c => c.Titulo)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres.");

        RuleFor(c => c.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(5000).WithMessage("Descrição deve ter no máximo 5000 caracteres.");
    }
}
