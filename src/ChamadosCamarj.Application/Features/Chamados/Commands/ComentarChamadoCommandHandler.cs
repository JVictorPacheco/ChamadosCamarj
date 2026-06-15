using MediatR;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class ComentarChamadoCommandHandler : IRequestHandler<ComentarChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;

    public ComentarChamadoCommandHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task Handle(ComentarChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.ChamadoId, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.ChamadoId);

        var tipo = request.Interno ? TipoComentario.Interno : TipoComentario.Publico;
        var comentario = new Comentario(request.ChamadoId, request.Autor, request.Conteudo, tipo);
        chamado.Comentarios.Add(comentario);

        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);
    }
}
