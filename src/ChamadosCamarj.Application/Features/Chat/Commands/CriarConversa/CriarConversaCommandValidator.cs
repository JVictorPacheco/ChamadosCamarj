using FluentValidation;

namespace ChamadosCamarj.Application.Features.Chat.Commands.CriarConversa;

public class CriarConversaCommandValidator : AbstractValidator<CriarConversaCommand>
{
    public CriarConversaCommandValidator()
    {
        RuleFor(c => c.DestinatarioId)
            .NotEmpty().WithMessage("ID do destinatário é obrigatório.");
    }
}
