using FluentValidation;

namespace ChamadosCamarj.Application.Features.Acessos.Commands;

/// <summary>Pedido inválido é recusado antes de gravar qualquer coisa (review R-05).</summary>
public class SalvarAcessosCommandValidator : AbstractValidator<SalvarAcessosCommand>
{
    public SalvarAcessosCommandValidator()
    {
        RuleFor(c => c.UsuarioId).NotEmpty();
        RuleFor(c => c.Modulos).NotNull().WithMessage("Informe os módulos.");
        RuleFor(c => c.ChatPerfil).IsInEnum().WithMessage("Nível de Chat inválido.");
    }
}

public class DefinirChatDeAcessoCommandValidator : AbstractValidator<DefinirChatDeAcessoCommand>
{
    public DefinirChatDeAcessoCommandValidator()
    {
        RuleFor(c => c.UsuarioId).NotEmpty();
        RuleFor(c => c.ChatPerfil).IsInEnum().WithMessage("Nível de Chat inválido.");
    }
}
