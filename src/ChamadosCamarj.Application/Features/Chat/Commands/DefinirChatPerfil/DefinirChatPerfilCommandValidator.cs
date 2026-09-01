using FluentValidation;

namespace ChamadosCamarj.Application.Features.Chat.Commands.DefinirChatPerfil;

public class DefinirChatPerfilCommandValidator : AbstractValidator<DefinirChatPerfilCommand>
{
    public DefinirChatPerfilCommandValidator()
    {
        RuleFor(c => c.UsuarioId)
            .NotEmpty().WithMessage("ID do usuário é obrigatório.");

        RuleFor(c => c.ChatPerfil)
            .IsInEnum().WithMessage("Perfil de chat inválido.");
    }
}
