using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class FecharChamadoCommandHandler : IRequestHandler<FecharChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;

    public FecharChamadoCommandHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task Handle(FecharChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        chamado.Fechar();
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);
    }
}
