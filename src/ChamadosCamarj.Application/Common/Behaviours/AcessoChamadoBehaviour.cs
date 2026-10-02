using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using MediatR;

namespace ChamadosCamarj.Application.Common.Behaviours;

/// <summary>
/// Aplica as regras de acesso a chamados antes de qualquer handler marcado com IRequerAcessoChamado,
/// usando o usuário logado (JWT): quem não pode VER recebe 404, sem revelar que o chamado existe
/// (AC-16); quem vê mas não pode realizar a ação recebe 403.
/// </summary>
public class AcessoChamadoBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly ICurrentUserService _currentUser;

    public AcessoChamadoBehaviour(IChamadoRepository chamadoRepository, ICurrentUserService currentUser)
    {
        _chamadoRepository = chamadoRepository;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IRequerAcessoChamado requisicao)
            return await next();

        var acesso = _currentUser.ObterContextoAcesso();

        if (!await _chamadoRepository.PodeVerAsync(requisicao.ChamadoId, acesso, cancellationToken))
            throw new NotFoundException("Chamado", requisicao.ChamadoId);

        var solicitanteEmail = string.Empty;
        if (ChamadoPermissoes.DependeDoSolicitante(requisicao.Acao) && acesso.Perfil == Perfil.Solicitante)
        {
            var chamado = await _chamadoRepository.ObterPorIdAsync(requisicao.ChamadoId, cancellationToken);
            solicitanteEmail = chamado?.SolicitanteEmail ?? string.Empty;
        }

        if (!ChamadoPermissoes.Pode(requisicao.Acao, acesso, solicitanteEmail))
            throw new ForbiddenException("Você não tem permissão para realizar esta ação neste chamado.");

        return await next();
    }
}
