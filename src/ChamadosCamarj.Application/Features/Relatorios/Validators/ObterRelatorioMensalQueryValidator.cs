using FluentValidation;
using ChamadosCamarj.Application.Features.Relatorios.Queries;

namespace ChamadosCamarj.Application.Features.Relatorios.Validators;

public class ObterRelatorioMensalQueryValidator : AbstractValidator<ObterRelatorioMensalQuery>
{
    public ObterRelatorioMensalQueryValidator()
    {
        RuleFor(q => q.Mes)
            .InclusiveBetween(1, 12).WithMessage("Mês deve estar entre 1 e 12.");

        RuleFor(q => q.Ano)
            .InclusiveBetween(2020, DateTime.UtcNow.Year + 1)
            .WithMessage($"Ano deve estar entre 2020 e {DateTime.UtcNow.Year + 1}.");
    }
}
