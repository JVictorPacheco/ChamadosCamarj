using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Categorias.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Categorias.Commands;

public class CriarCategoriaCommandHandler : IRequestHandler<CriarCategoriaCommand, CategoriaResponse>
{
    private readonly ICategoriaRepository _categoriaRepository;

    public CriarCategoriaCommandHandler(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<CategoriaResponse> Handle(CriarCategoriaCommand request, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var categorias = await _categoriaRepository.ObterTodasAsync(cancellationToken);
        if (categorias.Any(c => string.Equals(c.Nome, request.Nome.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException($"Já existe uma categoria com o nome '{request.Nome.Trim()}'.");

        var categoria = new Categoria(request.Nome, request.Descricao);

        await _categoriaRepository.AdicionarAsync(categoria, cancellationToken);

        return categoria.ToResponse();
    }
}
