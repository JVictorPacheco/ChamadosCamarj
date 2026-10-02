using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Interfaces;
using ChamadosCamarj.Domain.Interfaces;
using MediatR;

namespace ChamadosCamarj.Application.Common.Behaviours;

/// <summary>
/// Edição simultânea (spec correcoes-pre-deploy AC-09/AC-10): se o cliente informou a versão que
/// leu e o chamado mudou desde então, a alteração é recusada com 409. Roda depois do
/// AcessoChamadoBehaviour, para quem não vê o chamado continuar recebendo 404. Comentário e anexo
/// nunca são recusados (AC-13); sem versão informada, segue como antes (AC-23).
/// </summary>
public class VersaoChamadoBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IVersaoLidaAccessor _versaoLida;

    public VersaoChamadoBehaviour(IChamadoRepository chamadoRepository, IVersaoLidaAccessor versaoLida)
    {
        _chamadoRepository = chamadoRepository;
        _versaoLida = versaoLida;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IRequerAcessoChamado requisicao || !ChamadoPermissoes.AlteraChamado(requisicao.Acao))
            return await next();

        var versaoLida = _versaoLida.VersaoLida;
        if (string.IsNullOrWhiteSpace(versaoLida))
            return await next();

        var versaoAtual = await _chamadoRepository.ObterVersaoAsync(requisicao.ChamadoId, cancellationToken);
        if (versaoAtual is not null && versaoAtual != versaoLida)
            throw new ConflictException(VersaoChamado.MensagemConflito);

        return await next();
    }
}
