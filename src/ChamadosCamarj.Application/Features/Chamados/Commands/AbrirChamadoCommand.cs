using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public record AbrirChamadoCommand(
    string Titulo,
    string Descricao,
    string SolicitanteNome,
    string SolicitanteEmail,
    Guid AreaId,
    Guid TipoId,
    PrioridadeChamado Prioridade = PrioridadeChamado.Media
) : IRequest<ChamadoResponse>;
