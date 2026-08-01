using FluentValidation;
using ChamadosCamarj.Application.Features.Usuarios.Commands;

namespace ChamadosCamarj.Application.Features.Usuarios.Validators;

public class CriarUsuarioPerfilCommandValidator : AbstractValidator<CriarUsuarioPerfilCommand>
{
    public CriarUsuarioPerfilCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.")
            .MaximumLength(200);

        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(c => c.Perfil)
            .IsInEnum().WithMessage("Perfil inválido.");

        RuleFor(c => c.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres.");
    }
}
