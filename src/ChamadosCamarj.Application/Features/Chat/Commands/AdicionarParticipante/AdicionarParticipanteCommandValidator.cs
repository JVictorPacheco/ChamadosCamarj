using FluentValidation;

namespace ChamadosCamarj.Application.Features.Chat.Commands.AdicionarParticipante;

public class AdicionarParticipanteCommandValidator : AbstractValidator<AdicionarParticipanteCommand>
{
    public AdicionarParticipanteCommandValidator()
    {
        RuleFor(c => c.ConversaId).NotEmpty();
        RuleFor(c => c.UsuarioId).NotEmpty();
    }
}
