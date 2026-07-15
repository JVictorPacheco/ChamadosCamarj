using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.DTOs;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public class ListarHistoricoQueryHandler : IRequestHandler<ListarHistoricoQuery, IEnumerable<HistoricoResponse>>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IHistoricoRepository _historicoRepository;

    public ListarHistoricoQueryHandler(
        IChamadoRepository chamadoRepository,
        IHistoricoRepository historicoRepository)
    {
        _chamadoRepository = chamadoRepository;
        _historicoRepository = historicoRepository;
    }

    public async Task<IEnumerable<HistoricoResponse>> Handle(ListarHistoricoQuery request, CancellationToken cancellationToken)
    {
        // Verificar se o chamado existe
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.ChamadoId, cancellationToken)
            ?? throw new NotFoundException("Chamado", request.ChamadoId);

        var historicos = await _historicoRepository.ObterPorChamadoAsync(request.ChamadoId, cancellationToken);

        return historicos.Select(h => new HistoricoResponse(
            h.Id,
            h.ChamadoId,
            h.UsuarioNome,
            h.UsuarioId,
            h.Acao,
            h.DetalheAnterior,
            h.DetalheNovo,
            h.DataHora
        ));
    }
}
