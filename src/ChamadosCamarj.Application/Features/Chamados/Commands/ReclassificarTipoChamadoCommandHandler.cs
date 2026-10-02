using MediatR;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Interfaces;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class ReclassificarTipoChamadoCommandHandler : IRequestHandler<ReclassificarTipoChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly ITipoChamadoRepository _tipoRepository;
    private readonly IHistoricoRepository _historicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReclassificarTipoChamadoCommandHandler(
        IChamadoRepository chamadoRepository,
        ITipoChamadoRepository tipoRepository,
        IHistoricoRepository historicoRepository,
        IUnitOfWork unitOfWork)
    {
        _chamadoRepository = chamadoRepository;
        _tipoRepository = tipoRepository;
        _historicoRepository = historicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReclassificarTipoChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdComTrackingAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        var novoTipo = await _tipoRepository.ObterPorIdAsync(request.NovoTipoId, cancellationToken);
        if (novoTipo is null || !novoTipo.Ativo)
            throw new NotFoundException("Tipo", request.NovoTipoId);

        var tipoAnterior = chamado.Tipo?.Nome ?? "Não classificado";
        chamado.ReclassificarTipo(novoTipo.Id);

        await using var _ = _unitOfWork;
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);
        await _historicoRepository.AdicionarAsync(HistoricoEntrada.Criar(
            chamado.Id,
            request.UsuarioNome,
            request.UsuarioId,
            AcaoHistorico.TipoReclassificado,
            detalheAnterior: tipoAnterior,
            detalheNovo: novoTipo.Nome), cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
