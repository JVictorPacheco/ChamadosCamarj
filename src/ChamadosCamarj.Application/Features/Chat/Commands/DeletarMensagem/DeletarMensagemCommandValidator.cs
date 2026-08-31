using FluentValidation;

namespace ChamadosCamarj.Application.Features.Chat.Commands.DeletarMensagem;

public class DeletarMensagemCommandValidator : AbstractValidator<DeletarMensagemCommand>
{
    public DeletarMensagemCommandValidator()
    {
        RuleFor(c => c.MensagemId)
            .NotEmpty().WithMessage("ID da mensagem é obrigatório.");
    }
}
