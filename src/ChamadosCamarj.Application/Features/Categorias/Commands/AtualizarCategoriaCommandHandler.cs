using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Features.Categorias.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Categorias.Commands;

public class AtualizarCategoriaCommandHandler : IRequestHandler<AtualizarCategoriaCommand, CategoriaResponse?>
{
    private readonly ICategoriaRepository _categoriaRepository;

    public AtualizarCategoriaCommandHandler(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<CategoriaResponse?> Handle(AtualizarCategoriaCommand request, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var categoria = await _categoriaRepository.ObterPorIdAsync(request.Id, cancellationToken);
        if (categoria is null)
            return null;

        if (request.Ativa && !categoria.Ativa)
            categoria.Ativar();
        else if (!request.Ativa && categoria.Ativa)
            categoria.Desativar();

        categoria.Atualizar(request.Nome, request.Descricao);

        await _categoriaRepository.AtualizarAsync(categoria, cancellationToken);

        return categoria.ToResponse();
    }
}
