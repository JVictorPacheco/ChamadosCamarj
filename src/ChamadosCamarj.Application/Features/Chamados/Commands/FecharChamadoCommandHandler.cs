using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Application.Common.Extensions;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class FecharChamadoCommandHandler : IRequestHandler<FecharChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IHistoricoRepository _historicoRepository;
    private readonly IPublisher _publisher;

    public FecharChamadoCommandHandler(IChamadoRepository chamadoRepository, IHistoricoRepository historicoRepository, IPublisher publisher)
    {
        _chamadoRepository = chamadoRepository;
        _historicoRepository = historicoRepository;
        _publisher = publisher;
    }

    public async Task Handle(FecharChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        chamado.Fechar();
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);

        await _historicoRepository.RegistrarHistoricoAsync(
            chamado.Id,
            AcaoHistorico.Fechado,
            detalheNovo: "Chamado fechado",
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
