using FluentValidation;

namespace ChamadosCamarj.Application.Features.Chat.Commands.MarcarComoLido;

public class MarcarComoLidoCommandValidator : AbstractValidator<MarcarComoLidoCommand>
{
    public MarcarComoLidoCommandValidator()
    {
        RuleFor(c => c.ConversaId)
            .NotEmpty().WithMessage("ID da conversa é obrigatório.");
    }
}
