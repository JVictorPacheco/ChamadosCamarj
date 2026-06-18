using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class CancelarChamadoCommandHandler : IRequestHandler<CancelarChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;

    public CancelarChamadoCommandHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task Handle(CancelarChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        chamado.Cancelar();
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);
    }
}
