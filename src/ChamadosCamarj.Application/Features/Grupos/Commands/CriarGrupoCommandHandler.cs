using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Grupos.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Grupos.Commands;

public class CriarGrupoCommandHandler : IRequestHandler<CriarGrupoCommand, GrupoResponse>
{
    private readonly IGrupoRepository _grupoRepository;

    public CriarGrupoCommandHandler(IGrupoRepository grupoRepository)
    {
        _grupoRepository = grupoRepository;
    }

    public async Task<GrupoResponse> Handle(CriarGrupoCommand request, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var grupos = await _grupoRepository.ListarAsync(cancellationToken);
        if (grupos.Any(g => string.Equals(g.Nome, request.Nome.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException($"Já existe um grupo com o nome '{request.Nome.Trim()}'.");

        var grupo = new Grupo(request.Nome, request.Descricao);

        await _grupoRepository.AdicionarAsync(grupo, cancellationToken);

        return grupo.ToResponse();
    }
}
