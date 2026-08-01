using MediatR;
using ChamadosCamarj.Application.Features.Grupos.DTOs;

namespace ChamadosCamarj.Application.Features.Grupos.Commands;

public record CriarGrupoCommand(
    string Nome,
    string Descricao,
    string? PerfilRequisitante = null
) : IRequest<GrupoResponse>;
