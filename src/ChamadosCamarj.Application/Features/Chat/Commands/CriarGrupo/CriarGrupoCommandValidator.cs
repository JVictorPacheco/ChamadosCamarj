using FluentValidation;

namespace ChamadosCamarj.Application.Features.Chat.Commands.CriarGrupo;

public class CriarGrupoCommandValidator : AbstractValidator<CriarGrupoCommand>
{
    public CriarGrupoCommandValidator()
    {
        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("Nome do grupo é obrigatório.")
            .MaximumLength(150).WithMessage("Nome do grupo deve ter no máximo 150 caracteres.");

        RuleFor(c => c.ParticipanteIds)
            .NotNull().WithMessage("A lista de participantes é obrigatória.")
            .Must(ids => ids is not null && ids.Count >= 2)
            .WithMessage("Um grupo precisa de ao menos 2 participantes.");
    }
}
