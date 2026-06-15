using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class ResolverChamadoCommandHandler : IRequestHandler<ResolverChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;

    public ResolverChamadoCommandHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task Handle(ResolverChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        chamado.Resolver();
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);
    }
}
