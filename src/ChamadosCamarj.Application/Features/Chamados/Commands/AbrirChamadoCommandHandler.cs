using MediatR;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class AbrirChamadoCommandHandler : IRequestHandler<AbrirChamadoCommand, ChamadoResponse>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IPublisher _publisher;

    public AbrirChamadoCommandHandler(
        IChamadoRepository chamadoRepository,
        ICategoriaRepository categoriaRepository,
        IPublisher publisher)
    {
        _chamadoRepository = chamadoRepository;
        _categoriaRepository = categoriaRepository;
        _publisher = publisher;
    }

    public async Task<ChamadoResponse> Handle(AbrirChamadoCommand request, CancellationToken cancellationToken)
    {
        var categoriaExiste = await _categoriaRepository.ExisteAsync(request.CategoriaId, cancellationToken);
        if (!categoriaExiste)
            throw new NotFoundException("Categoria", request.CategoriaId);

        var chamado = new Chamado(
            request.Titulo,
            request.Descricao,
            request.SolicitanteNome,
            request.SolicitanteEmail,
            request.CategoriaId,
            request.Prioridade
        );

        await _chamadoRepository.AdicionarAsync(chamado, cancellationToken);

        await _publisher.Publish(new ChamadoCriadoNotification(
            chamado.Id,
            chamado.Titulo,
            StatusChamadoNotification.Aberto
        ), cancellationToken);

        return chamado.ToResponse();
    }
}
