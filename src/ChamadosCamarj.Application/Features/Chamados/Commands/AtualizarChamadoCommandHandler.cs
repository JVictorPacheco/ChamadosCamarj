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
        // Com tracking, como os demais handlers de escrita desde o controle de concorrência (2026-07-31):
        // sem ele, o EF compara DataAtualizacao com o valor já alterado e todo save dava 409.
        var chamado = await _chamadoRepository.ObterPorIdComTrackingAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.Id);

        chamado.AtualizarDados(request.Titulo, request.Descricao);
        await _chamadoRepository.AtualizarAsync(chamado, cancellationToken);
    }
}
