using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public class ListarAnexosQueryHandler : IRequestHandler<ListarAnexosQuery, IEnumerable<AnexoResponse>>
{
    private readonly IChamadoRepository _chamadoRepository;

    public ListarAnexosQueryHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task<IEnumerable<AnexoResponse>> Handle(ListarAnexosQuery request, CancellationToken cancellationToken)
    {
        var anexos = await _chamadoRepository.ObterAnexosPorChamadoAsync(request.ChamadoId, cancellationToken);

        return anexos.Select(a => new AnexoResponse(a.Id, a.NomeArquivo, a.TipoArquivo, a.TamanhoBytes, a.EnviadoPorNome, a.DataCriacao));
    }
}
