using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class AtualizarChamadoCommandHandler : IRequestHandler<AtualizarChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;

    public AtualizarChamadoCommandHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task Handle(AtualizarChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        chamado.AtualizarDados(request.Titulo, request.Descricao);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);
    }
}
