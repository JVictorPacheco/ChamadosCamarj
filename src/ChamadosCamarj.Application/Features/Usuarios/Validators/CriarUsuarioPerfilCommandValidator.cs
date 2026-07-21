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
    }
}
