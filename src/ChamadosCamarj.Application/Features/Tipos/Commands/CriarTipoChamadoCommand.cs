using MediatR;
using ChamadosCamarj.Application.Features.Tipos.DTOs;

namespace ChamadosCamarj.Application.Features.Tipos.Commands;

public record CriarTipoChamadoCommand(
    string Nome,
    string Descricao,
    string? PerfilRequisitante = null
) : IRequest<TipoChamadoResponse>;
