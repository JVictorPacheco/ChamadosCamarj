using FluentValidation;

namespace ChamadosCamarj.Application.Features.Chat.Commands.RemoverParticipante;

public class RemoverParticipanteCommandValidator : AbstractValidator<RemoverParticipanteCommand>
{
    public RemoverParticipanteCommandValidator()
    {
        RuleFor(c => c.ConversaId).NotEmpty();
        RuleFor(c => c.UsuarioId).NotEmpty();
    }
}
