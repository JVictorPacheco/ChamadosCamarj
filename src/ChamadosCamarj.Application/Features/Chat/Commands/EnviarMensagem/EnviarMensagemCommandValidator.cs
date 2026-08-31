using FluentValidation;

namespace ChamadosCamarj.Application.Features.Chat.Commands.EnviarMensagem;

public class EnviarMensagemCommandValidator : AbstractValidator<EnviarMensagemCommand>
{
    public EnviarMensagemCommandValidator()
    {
        RuleFor(c => c.ConversaId)
            .NotEmpty().WithMessage("ID da conversa é obrigatório.");

        RuleFor(c => c.Conteudo)
            .NotEmpty().WithMessage("A mensagem não pode ser vazia.")
            .MaximumLength(5000).WithMessage("A mensagem deve ter no máximo 5000 caracteres.");
    }
}
