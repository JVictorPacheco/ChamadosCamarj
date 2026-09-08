using FluentValidation;

namespace ChamadosCamarj.Application.Features.Chat.Commands.DefinirPreferenciaLeitura;

public class DefinirPreferenciaLeituraCommandValidator : AbstractValidator<DefinirPreferenciaLeituraCommand>
{
    public DefinirPreferenciaLeituraCommandValidator()
    {
        RuleFor(c => c.UsuarioId)
            .NotEmpty().WithMessage("Usuário autenticado é obrigatório.");
    }
}
