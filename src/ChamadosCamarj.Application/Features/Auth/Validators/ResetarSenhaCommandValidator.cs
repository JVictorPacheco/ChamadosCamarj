using FluentValidation;
using ChamadosCamarj.Application.Features.Auth.Commands;

namespace ChamadosCamarj.Application.Features.Auth.Validators;

public class ResetarSenhaCommandValidator : AbstractValidator<ResetarSenhaCommand>
{
    public ResetarSenhaCommandValidator()
    {
        RuleFor(c => c.Token)
            .NotEmpty().WithMessage("Token é obrigatório.");

        RuleFor(c => c.NovaSenha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("A senha deve ter no mínimo 8 caracteres.");
    }
}
