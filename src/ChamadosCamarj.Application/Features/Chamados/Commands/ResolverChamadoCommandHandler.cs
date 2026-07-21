using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Application.Common.Extensions;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class ResolverChamadoCommandHandler : IRequestHandler<ResolverChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IHistoricoRepository _historicoRepository;
    private readonly IPublisher _publisher;

    public ResolverChamadoCommandHandler(IChamadoRepository chamadoRepository, IHistoricoRepository historicoRepository, IPublisher publisher)
    {
        _chamadoRepository = chamadoRepository;
        _historicoRepository = historicoRepository;
        _publisher = publisher;
    }

    public async Task Handle(ResolverChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        chamado.Resolver();
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);

        await _historicoRepository.RegistrarHistoricoAsync(
            chamado.Id,
            AcaoHistorico.Resolvido,
            detalheNovo: "Chamado resolvido",
            usuarioNome: request.UsuarioNome,
            usuarioId: request.UsuarioId,
            cancellationToken: cancellationToken
        );

        await _publisher.Publish(new StatusAlteradoNotification(
            chamado.Id,
            chamado.Status.ToString(),
            chamado.DataAtualizacao ?? DateTime.UtcNow
        ), cancellationToken);
    }
}
