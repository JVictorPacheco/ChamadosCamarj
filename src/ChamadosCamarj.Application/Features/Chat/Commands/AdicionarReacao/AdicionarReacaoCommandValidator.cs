using FluentValidation;

namespace ChamadosCamarj.Application.Features.Chat.Commands.AdicionarReacao;

public class AdicionarReacaoCommandValidator : AbstractValidator<AdicionarReacaoCommand>
{
    public AdicionarReacaoCommandValidator()
    {
        RuleFor(c => c.MensagemId)
            .NotEmpty().WithMessage("ID da mensagem é obrigatório.");

        RuleFor(c => c.Emoji)
            .NotEmpty().WithMessage("Emoji é obrigatório.")
            .MaximumLength(20).WithMessage("Emoji inválido.");
    }
}
