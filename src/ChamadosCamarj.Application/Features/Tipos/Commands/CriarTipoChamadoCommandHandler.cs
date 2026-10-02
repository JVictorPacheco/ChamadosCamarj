using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Tipos.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Tipos.Commands;

public class CriarTipoChamadoCommandHandler : IRequestHandler<CriarTipoChamadoCommand, TipoChamadoResponse>
{
    private readonly ITipoChamadoRepository _tipoRepository;

    public CriarTipoChamadoCommandHandler(ITipoChamadoRepository tipoRepository)
    {
        _tipoRepository = tipoRepository;
    }

    public async Task<TipoChamadoResponse> Handle(CriarTipoChamadoCommand request, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var tipos = await _tipoRepository.ObterTodosAsync(cancellationToken);
        if (tipos.Any(c => string.Equals(c.Nome, request.Nome.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException($"Já existe um tipo de chamado com o nome '{request.Nome.Trim()}'.");

        var tipo = new TipoChamado(request.Nome, request.Descricao);

        await _tipoRepository.AdicionarAsync(tipo, cancellationToken);

        return tipo.ToResponse();
    }
}
