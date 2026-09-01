using FluentValidation;

namespace ChamadosCamarj.Application.Features.Chat.Commands.EditarMensagem;

public class EditarMensagemCommandValidator : AbstractValidator<EditarMensagemCommand>
{
    public EditarMensagemCommandValidator()
    {
        RuleFor(c => c.MensagemId)
            .NotEmpty().WithMessage("ID da mensagem é obrigatório.");

        RuleFor(c => c.NovoConteudo)
            .NotEmpty().WithMessage("A mensagem não pode ser vazia.")
            .MaximumLength(5000).WithMessage("A mensagem deve ter no máximo 5000 caracteres.");
    }
}
