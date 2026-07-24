using MediatR;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chamados.DTOs;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class ComentarChamadoCommandHandler : IRequestHandler<ComentarChamadoCommand, ComentarioResponse>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IPublisher _publisher;

    public ComentarChamadoCommandHandler(IChamadoRepository chamadoRepository, IPublisher publisher)
    {
        _chamadoRepository = chamadoRepository;
        _publisher = publisher;
    }

    public async Task<ComentarioResponse> Handle(ComentarChamadoCommand request, CancellationToken cancellationToken)
    {
        var existe = await _chamadoRepository.ExisteAsync(request.ChamadoId, cancellationToken);
        if (!existe)
            throw new NotFoundException("Chamado", request.ChamadoId);

        var tipo = request.Interno ? TipoComentario.Interno : TipoComentario.Publico;
        var comentario = new Comentario(request.ChamadoId, request.Autor, request.Conteudo, tipo);

        await _chamadoRepository.AdicionarComentarioAsync(comentario, cancellationToken);

        await _publisher.Publish(new ComentarioAdicionadoNotification(
            request.ChamadoId,
            request.Autor,
            request.Conteudo
        ), cancellationToken);

        return new ComentarioResponse(comentario.Id, comentario.Autor, comentario.Conteudo, comentario.Tipo, comentario.DataCriacao);
    }
}
