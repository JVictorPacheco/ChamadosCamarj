using FluentValidation;
using ChamadosCamarj.Application.Features.Usuarios.Commands;

namespace ChamadosCamarj.Application.Features.Usuarios.Validators;

public class RedefinirSenhaCommandValidator : AbstractValidator<RedefinirSenhaCommand>
{
    public RedefinirSenhaCommandValidator()
    {
        RuleFor(c => c.NovaSenha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres.");
    }
}
