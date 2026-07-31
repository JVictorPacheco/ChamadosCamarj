using MediatR;
using ChamadosCamarj.Application.Features.Categorias.DTOs;

namespace ChamadosCamarj.Application.Features.Categorias.Commands;

public record AtualizarCategoriaCommand(
    Guid Id,
    string Nome,
    string Descricao,
    bool Ativa,
    string? PerfilRequisitante = null
) : IRequest<CategoriaResponse?>;
