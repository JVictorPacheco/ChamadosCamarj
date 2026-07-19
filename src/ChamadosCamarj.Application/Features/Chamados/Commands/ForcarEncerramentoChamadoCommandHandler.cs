using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class ForcarEncerramentoChamadoCommandHandler : IRequestHandler<ForcarEncerramentoChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IHistoricoRepository _historicoRepository;
    private readonly IPublisher _publisher;

    public ForcarEncerramentoChamadoCommandHandler(
        IChamadoRepository chamadoRepository,
        IHistoricoRepository historicoRepository,
        IPublisher publisher)
    {
        _chamadoRepository = chamadoRepository;
        _historicoRepository = historicoRepository;
        _publisher = publisher;
    }

    public async Task Handle(ForcarEncerramentoChamadoCommand request, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        var statusAnterior = chamado.Status;

        chamado.ForcarEncerramento();
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);

        var historico = HistoricoEntrada.Criar(
            chamado.Id,
            request.UsuarioNome,
            request.UsuarioId,
            AcaoHistorico.EncerramentoForcado,
            detalheAnterior: statusAnterior.ToString(),
            detalheNovo: request.Motivo.Trim()
        );
        await _historicoRepository.AdicionarAsync(historico, cancellationToken);

        await _publisher.Publish(new StatusAlteradoNotification(
            chamado.Id,
            chamado.Status.ToString(),
            chamado.DataAtualizacao ?? DateTime.UtcNow
        ), cancellationToken);
    }
}
