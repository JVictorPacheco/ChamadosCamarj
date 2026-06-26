using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class AlterarStatusChamadoCommandHandler : IRequestHandler<AlterarStatusChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;

    public AlterarStatusChamadoCommandHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task Handle(AlterarStatusChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        chamado.AlterarStatus(request.NovoStatus);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);
    }
}
