using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Common.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class ReatribuirChamadoCommandHandler : IRequestHandler<ReatribuirChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IHistoricoRepository _historicoRepository;
    private readonly IPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;

    public ReatribuirChamadoCommandHandler(
        IChamadoRepository chamadoRepository,
        IHistoricoRepository historicoRepository,
        IPublisher publisher,
        IUnitOfWork unitOfWork)
    {
        _chamadoRepository = chamadoRepository;
        _historicoRepository = historicoRepository;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReatribuirChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdComTrackingAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        var responsavelAnterior = chamado.ResponsavelNome ?? "Não atribuído";

        chamado.Reatribuir(request.NovoResponsavelId, request.NovoResponsavelNome);

        await using var _ = _unitOfWork;
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);

        // Gerar entrada no histórico
        var historico = HistoricoEntrada.Criar(
            chamado.Id,
            request.UsuarioNome,
            request.UsuarioId,
            AcaoHistorico.Reatribuido,
            detalheAnterior: responsavelAnterior,
            detalheNovo: request.NovoResponsavelNome
        );
        await _historicoRepository.AdicionarAsync(historico, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _publisher.Publish(new StatusAlteradoNotification(
            chamado.Id,
            chamado.Status.ToString(),
            chamado.DataAtualizacao ?? DateTime.UtcNow
        ), cancellationToken);
    }
}
