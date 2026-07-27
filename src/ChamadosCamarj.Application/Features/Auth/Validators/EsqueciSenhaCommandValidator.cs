using FluentValidation;
using ChamadosCamarj.Application.Features.Auth.Commands;

namespace ChamadosCamarj.Application.Features.Auth.Validators;

public class EsqueciSenhaCommandValidator : AbstractValidator<EsqueciSenhaCommand>
{
    public EsqueciSenhaCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");
    }
}
