using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Features.Grupos.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Grupos.Commands;

public class AtualizarGrupoCommandHandler : IRequestHandler<AtualizarGrupoCommand, GrupoResponse?>
{
    private readonly IGrupoRepository _grupoRepository;

    public AtualizarGrupoCommandHandler(IGrupoRepository grupoRepository)
    {
        _grupoRepository = grupoRepository;
    }

    public async Task<GrupoResponse?> Handle(AtualizarGrupoCommand request, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var grupo = await _grupoRepository.ObterPorIdAsync(request.Id, cancellationToken);
        if (grupo is null)
            return null;

        if (request.Ativo && !grupo.Ativo)
            grupo.Ativar();
        else if (!request.Ativo && grupo.Ativo)
            grupo.Desativar();

        grupo.Atualizar(request.Nome, request.Descricao);

        await _grupoRepository.AtualizarAsync(grupo, cancellationToken);

        return grupo.ToResponse();
    }
}
