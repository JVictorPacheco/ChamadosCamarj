using MediatR;
using ChamadosCamarj.Application.Features.Grupos.DTOs;

namespace ChamadosCamarj.Application.Features.Grupos.Commands;

public record AtualizarGrupoCommand(
    Guid Id,
    string Nome,
    string Descricao,
    bool Ativo,
    string? PerfilRequisitante = null
) : IRequest<GrupoResponse?>;
