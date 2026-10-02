using MediatR;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Common.Extensions;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Application.Common.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class AbrirChamadoCommandHandler : IRequestHandler<AbrirChamadoCommand, ChamadoResponse>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IGrupoRepository _grupoRepository;
    private readonly ITipoChamadoRepository _tipoRepository;
    private readonly IHistoricoRepository _historicoRepository;
    private readonly IPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;

    public AbrirChamadoCommandHandler(
        IChamadoRepository chamadoRepository,
        IGrupoRepository grupoRepository,
        ITipoChamadoRepository tipoRepository,
        IHistoricoRepository historicoRepository,
        IPublisher publisher,
        IUnitOfWork unitOfWork)
    {
        _chamadoRepository = chamadoRepository;
        _grupoRepository = grupoRepository;
        _tipoRepository = tipoRepository;
        _historicoRepository = historicoRepository;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
    }

    public async Task<ChamadoResponse> Handle(AbrirChamadoCommand request, CancellationToken cancellationToken)
    {
        var area = await _grupoRepository.ObterPorIdAsync(request.AreaId, cancellationToken);
        if (area is null || !area.Ativo)
            throw new NotFoundException("Área", request.AreaId);

        var tipo = await _tipoRepository.ObterPorIdAsync(request.TipoId, cancellationToken);
        if (tipo is null || !tipo.Ativo)
            throw new NotFoundException("Tipo", request.TipoId);

        var chamado = new Chamado(
            request.Titulo,
            request.Descricao,
            request.SolicitanteNome,
            request.SolicitanteEmail,
            request.AreaId,
            request.TipoId,
            request.Prioridade
        );

        await using var _ = _unitOfWork;
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        await _chamadoRepository.AdicionarAsync(chamado, cancellationToken);

        // Registrar no histórico
        await _historicoRepository.RegistrarHistoricoAsync(
            chamado.Id,
            AcaoHistorico.Criado,
            detalheNovo: $"Chamado aberto por {request.SolicitanteNome}",
            usuarioNome: request.SolicitanteNome,
            cancellationToken: cancellationToken
        );

        await _unitOfWork.CommitAsync(cancellationToken);

        await _publisher.Publish(new ChamadoCriadoNotification(
            chamado.Id,
            chamado.Titulo,
            StatusChamadoNotification.Aberto
        ), cancellationToken);

        // Chamado recém-criado não tem as navegações carregadas: os nomes vêm da área e do tipo já lidos.
        return chamado.ToResponse(incluirInternos: false) with { AreaNome = area.Nome, TipoNome = tipo.Nome };
    }
}
