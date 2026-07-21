using FluentValidation;
using ChamadosCamarj.Application.Features.Chamados.Queries;

namespace ChamadosCamarj.Application.Features.Chamados.Validators;

public class ListarChamadosQueryValidator : AbstractValidator<ListarChamadosQuery>
{
    public ListarChamadosQueryValidator()
    {
        RuleFor(q => q.Pagina)
            .GreaterThan(0).WithMessage("Página deve ser maior que zero.");

        RuleFor(q => q.TamanhoPagina)
            .InclusiveBetween(1, 100).WithMessage("Tamanho da página deve estar entre 1 e 100.");

        RuleFor(q => q.DataFim)
            .Must((query, dataFim) => !query.DataInicio.HasValue || !dataFim.HasValue || dataFim >= query.DataInicio)
            .WithMessage("Data final deve ser maior ou igual à data inicial.");
    }
}
