using MediatR;
using ChamadosCamarj.Application.Features.Tipos.DTOs;

namespace ChamadosCamarj.Application.Features.Tipos.Commands;

public record AtualizarTipoChamadoCommand(
    Guid Id,
    string Nome,
    string Descricao,
    bool Ativo,
    string? PerfilRequisitante = null
) : IRequest<TipoChamadoResponse?>;
