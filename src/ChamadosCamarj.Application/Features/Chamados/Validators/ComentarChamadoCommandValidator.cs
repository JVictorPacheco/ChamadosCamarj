using FluentValidation;
using ChamadosCamarj.Application.Features.Chamados.Commands;

namespace ChamadosCamarj.Application.Features.Chamados.Validators;

public class ComentarChamadoCommandValidator : AbstractValidator<ComentarChamadoCommand>
{
    public ComentarChamadoCommandValidator()
    {
        RuleFor(c => c.ChamadoId)
            .NotEmpty().WithMessage("ID do chamado é obrigatório.");

        RuleFor(c => c.Autor)
            .NotEmpty().WithMessage("Autor é obrigatório.")
            .MaximumLength(150).WithMessage("Autor deve ter no máximo 150 caracteres.");

        RuleFor(c => c.Conteudo)
            .NotEmpty().WithMessage("Conteúdo do comentário é obrigatório.")
            .MaximumLength(10000).WithMessage("Conteúdo deve ter no máximo 10000 caracteres.");
    }
}
