using FluentValidation;
using ChamadosCamarj.Application.Features.Usuarios.Commands;

namespace ChamadosCamarj.Application.Features.Usuarios.Validators;

public class AtualizarUsuarioPerfilCommandValidator : AbstractValidator<AtualizarUsuarioPerfilCommand>
{
    public AtualizarUsuarioPerfilCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("ID do usuário é obrigatório.");

        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(c => c.Perfil)
            .IsInEnum().WithMessage("Perfil inválido.");
    }
}
