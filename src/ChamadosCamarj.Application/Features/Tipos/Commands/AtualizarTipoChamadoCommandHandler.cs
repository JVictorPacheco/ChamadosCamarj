using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Tipos.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Tipos.Commands;

public class AtualizarTipoChamadoCommandHandler : IRequestHandler<AtualizarTipoChamadoCommand, TipoChamadoResponse?>
{
    private readonly ITipoChamadoRepository _tipoRepository;

    public AtualizarTipoChamadoCommandHandler(ITipoChamadoRepository tipoRepository)
    {
        _tipoRepository = tipoRepository;
    }

    public async Task<TipoChamadoResponse?> Handle(AtualizarTipoChamadoCommand request, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var tipo = await _tipoRepository.ObterPorIdAsync(request.Id, cancellationToken);
        if (tipo is null)
            return null;

        var todos = await _tipoRepository.ObterTodosAsync(cancellationToken);
        if (todos.Any(t => t.Id != tipo.Id && string.Equals(t.Nome, request.Nome.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException($"Já existe um tipo de chamado com o nome '{request.Nome.Trim()}'.");

        if (request.Ativo && !tipo.Ativo)
            tipo.Ativar();
        else if (!request.Ativo && tipo.Ativo)
            tipo.Desativar();

        tipo.Atualizar(request.Nome, request.Descricao);

        await _tipoRepository.AtualizarAsync(tipo, cancellationToken);

        return tipo.ToResponse();
    }
}
