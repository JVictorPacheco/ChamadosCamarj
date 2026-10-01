using MediatR;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public class ObterChamadoPorIdQueryHandler : IRequestHandler<ObterChamadoPorIdQuery, ChamadoResponse?>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly ICurrentUserService _currentUser;

    public ObterChamadoPorIdQueryHandler(IChamadoRepository chamadoRepository, ICurrentUserService currentUser)
    {
        _chamadoRepository = chamadoRepository;
        _currentUser = currentUser;
    }

    public async Task<ChamadoResponse?> Handle(ObterChamadoPorIdQuery request, CancellationToken cancellationToken)
    {
        var chamado = await _chamadoRepository.ObterPorIdAsync(request.Id, cancellationToken);
        var incluirInternos = _currentUser.ObterContextoAcesso().Perfil != Perfil.Solicitante;
        return chamado?.ToResponse(incluirInternos);
    }
}
