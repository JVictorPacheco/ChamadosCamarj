using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Application.Features.Relatorios.DTOs;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using MediatR;

namespace ChamadosCamarj.Application.Features.Relatorios.Queries;

/// <summary>
/// Relatório mensal pedido pela tela: confere o módulo e decide de quem são os números antes de montar o
/// relatório (autorizacao-chamados AC-21 + controle-de-acesso AC-07/AC-12). Tirado do controller para a
/// ligação entre as três coisas ser testável (review-2 R-06).
/// </summary>
public record ObterRelatorioMensalAutorizadoQuery(int Ano, int Mes, Guid? ResponsavelId = null) : IRequest<RelatorioMensalResponse>;

public class ObterRelatorioMensalAutorizadoQueryHandler : IRequestHandler<ObterRelatorioMensalAutorizadoQuery, RelatorioMensalResponse>
{
    public const string MensagemSemAcesso = "Você não tem permissão para acessar o relatório.";

    private readonly ModuloGuard _moduloGuard;
    private readonly ICurrentUserService _currentUser;
    private readonly IChamadoRepository _chamados;
    private readonly IMediator _mediator;

    public ObterRelatorioMensalAutorizadoQueryHandler(ModuloGuard moduloGuard, ICurrentUserService currentUser, IChamadoRepository chamados, IMediator mediator)
    {
        _moduloGuard = moduloGuard;
        _currentUser = currentUser;
        _chamados = chamados;
        _mediator = mediator;
    }

    public async Task<RelatorioMensalResponse> Handle(ObterRelatorioMensalAutorizadoQuery request, CancellationToken cancellationToken)
    {
        await _moduloGuard.ExigirAsync(ModuloSistema.RelatorioMensal, MensagemSemAcesso, cancellationToken);

        var (responsavelId, idsVisiveis) = await RelatorioEscopo.ResolverAsync(
            _currentUser.ObterContextoAcesso(), request.ResponsavelId, _chamados, cancellationToken);

        return await _mediator.Send(new ObterRelatorioMensalQuery(request.Ano, request.Mes, responsavelId, idsVisiveis), cancellationToken);
    }
}
