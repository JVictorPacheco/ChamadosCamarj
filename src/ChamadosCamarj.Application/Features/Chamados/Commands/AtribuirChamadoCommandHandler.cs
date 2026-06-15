using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class AtribuirChamadoCommandHandler : IRequestHandler<AtribuirChamadoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;

    public AtribuirChamadoCommandHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task Handle(AtribuirChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        chamado.Atribuir(request.ResponsavelId, request.ResponsavelNome);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);
    }
}
