using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.DTOs;

public record AbrirChamadoRequest(
    string Titulo,
    string Descricao,
    string SolicitanteNome,
    string SolicitanteEmail,
    Guid CategoriaId,
    PrioridadeChamado Prioridade = PrioridadeChamado.Media
);
