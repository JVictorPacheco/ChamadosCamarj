using FluentValidation;
using ChamadosCamarj.Application.Features.Auth.Commands;

namespace ChamadosCamarj.Application.Features.Auth.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(c => c.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.");
    }
}
