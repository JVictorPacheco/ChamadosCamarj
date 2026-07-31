using FluentValidation;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.Validators;

public class ForcarEncerramentoChamadoCommandValidator : AbstractValidator<ForcarEncerramentoChamadoCommand>
{
    public ForcarEncerramentoChamadoCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("ID do chamado é obrigatório.");

        RuleFor(c => c.Motivo)
            .NotEmpty().WithMessage("Motivo é obrigatório.")
            .IsInEnum().WithMessage("Motivo inválido.");

        When(c => c.Motivo == MotivoEncerramento.Outro, () =>
        {
            RuleFor(c => c.MotivoOutro)
                .NotEmpty().WithMessage("Descreva o motivo quando selecionar 'Outro'.")
                .MinimumLength(5).WithMessage("Descrição deve ter no mínimo 5 caracteres.")
                .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres.");
        });
    }
}