using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Extensions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Common.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class AlterarPrioridadeChamadoCommandHandler : IRequestHandler<AlterarPrioridadeChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IHistoricoRepository _historicoRepository;
    private readonly IPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;

    public AlterarPrioridadeChamadoCommandHandler(
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

    public async Task Handle(AlterarPrioridadeChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdComTrackingAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        if (!Enum.TryParse<PrioridadeChamado>(request.NovaPrioridade, true, out var novaPrioridade))
            throw new BadRequestException($"Prioridade '{request.NovaPrioridade}' inválida.");

        var prioridadeAnterior = chamado.Prioridade.ToString();

        chamado.AlterarPrioridade(novaPrioridade);

        await using var _ = _unitOfWork;
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);

        // Gerar entrada no histórico
        var historico = HistoricoEntrada.Criar(
            chamado.Id,
            request.UsuarioNome,
            request.UsuarioId,
            AcaoHistorico.PrioridadeAlterada,
            detalheAnterior: prioridadeAnterior,
            detalheNovo: novaPrioridade.ToString()
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
