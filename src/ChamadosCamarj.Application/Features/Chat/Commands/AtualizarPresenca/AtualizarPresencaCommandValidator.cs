using FluentValidation;

namespace ChamadosCamarj.Application.Features.Chat.Commands.AtualizarPresenca;

public class AtualizarPresencaCommandValidator : AbstractValidator<AtualizarPresencaCommand>
{
    public AtualizarPresencaCommandValidator()
    {
        // Status null representa heartbeat (marca Online). Quando informado, precisa ser um valor válido do enum.
        RuleFor(c => c.Status)
            .IsInEnum().WithMessage("Status de presença inválido.")
            .When(c => c.Status.HasValue);
    }
}
