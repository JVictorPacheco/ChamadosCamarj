using ChamadosCamarj.Application.Common.Autorizacao;
using MediatR;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Dashboard.DTOs;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Dashboard.Queries;

public class ObterDistribuicaoQueryHandler : IRequestHandler<ObterDistribuicaoQuery, DistribuicaoResponse>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ModuloGuard _moduloGuard;

    public ObterDistribuicaoQueryHandler(IChamadoRepository chamadoRepository, ICurrentUserService currentUser, ModuloGuard moduloGuard)
    {
        _moduloGuard = moduloGuard;
        _chamadoRepository = chamadoRepository;
        _currentUser = currentUser;
    }

    public async Task<DistribuicaoResponse> Handle(ObterDistribuicaoQuery request, CancellationToken cancellationToken)
    {
        // Dashboard é só de Atendente/Admin; os números contam só o que o usuário pode ver (AC-13).
        var acesso = _currentUser.ObterContextoAcesso();
        // Quem não tem o módulo Dashboard é recusado; quem tem vê só os chamados que já vê (a visibilidade
        // entra pelo ContextoAcesso). Spec controle-de-acesso AC-07/AC-12 — antes: "Solicitante → 403".
        await _moduloGuard.ExigirAsync(ModuloSistema.Dashboard, "Você não tem permissão para acessar o dashboard.", cancellationToken);

        var contagens = await _chamadoRepository.ContarPorStatusAgrupadoAsync(acesso, cancellationToken);

        return new DistribuicaoResponse(
            contagens.GetValueOrDefault(StatusChamado.Aberto),
            contagens.GetValueOrDefault(StatusChamado.EmAndamento),
            contagens.GetValueOrDefault(StatusChamado.Resolvido),
            contagens.GetValueOrDefault(StatusChamado.Fechado),
            contagens.GetValueOrDefault(StatusChamado.Cancelado)
        );
    }
}
