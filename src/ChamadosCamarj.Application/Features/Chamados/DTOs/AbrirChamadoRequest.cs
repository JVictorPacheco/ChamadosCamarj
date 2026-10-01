using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.DTOs;

public record AbrirChamadoRequest(
    string Titulo,
    string Descricao,
    // Ignorados pelo servidor: o solicitante é sempre o usuário logado (spec autorizacao-chamados AC-12).
    // Mantidos opcionais só para não quebrar clientes que ainda os enviam.
    string? SolicitanteNome,
    string? SolicitanteEmail,
    Guid CategoriaId,
    PrioridadeChamado Prioridade = PrioridadeChamado.Media
);
