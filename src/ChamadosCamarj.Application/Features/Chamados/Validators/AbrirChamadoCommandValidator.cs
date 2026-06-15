using FluentValidation;
using ChamadosCamarj.Application.Features.Chamados.Commands;

namespace ChamadosCamarj.Application.Features.Chamados.Validators;

public class AbrirChamadoCommandValidator : AbstractValidator<AbrirChamadoCommand>
{
    public AbrirChamadoCommandValidator()
    {
        RuleFor(c => c.Titulo)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres.");

        RuleFor(c => c.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(5000).WithMessage("Descrição deve ter no máximo 5000 caracteres.");

        RuleFor(c => c.SolicitanteNome)
            .NotEmpty().WithMessage("Nome do solicitante é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(c => c.SolicitanteEmail)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.")
            .MaximumLength(200);

        RuleFor(c => c.CategoriaId)
            .NotEmpty().WithMessage("Categoria é obrigatória.");
    }
}
