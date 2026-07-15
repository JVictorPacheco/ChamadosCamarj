using FluentValidation;
using ChamadosCamarj.Application.Features.Chamados.Commands;

namespace ChamadosCamarj.Application.Features.Chamados.Validators;

public class AlterarPrioridadeChamadoCommandValidator : AbstractValidator<AlterarPrioridadeChamadoCommand>
{
    public AlterarPrioridadeChamadoCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("ID do chamado é obrigatório.");

        RuleFor(c => c.NovaPrioridade)
            .NotEmpty().WithMessage("Prioridade é obrigatória.")
            .Must(p => new[] { "Urgente", "Alta", "Media", "Baixa" }.Contains(p, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Prioridade deve ser: Urgente, Alta, Media ou Baixa.");
    }
}
