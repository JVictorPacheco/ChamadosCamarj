using MediatR;
using ChamadosCamarj.Application.Features.Tipos.DTOs;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Tipos.Queries;

public class ListarTiposChamadoQueryHandler : IRequestHandler<ListarTiposChamadoQuery, IEnumerable<TipoChamadoResponse>>
{
    private readonly ITipoChamadoRepository _tipoRepository;

    public ListarTiposChamadoQueryHandler(ITipoChamadoRepository tipoRepository)
    {
        _tipoRepository = tipoRepository;
    }

    public async Task<IEnumerable<TipoChamadoResponse>> Handle(ListarTiposChamadoQuery request, CancellationToken cancellationToken)
    {
        var tipos = request.ApenasAtivos == true
            ? await _tipoRepository.ObterAtivosAsync(cancellationToken)
            : await _tipoRepository.ObterTodosAsync(cancellationToken);

        return tipos.Select(c => new TipoChamadoResponse(c.Id, c.Nome, c.Descricao, c.Ativo));
    }
}
