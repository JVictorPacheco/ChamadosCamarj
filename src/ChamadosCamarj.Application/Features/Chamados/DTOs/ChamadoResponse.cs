using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.DTOs;

public record ChamadoResponse(
    Guid Id,
    string Titulo,
    string Descricao,
    StatusChamado Status,
    PrioridadeChamado Prioridade,
    string SolicitanteNome,
    string SolicitanteEmail,
    Guid? ResponsavelId,
    string? ResponsavelNome,
    Guid CategoriaId,
    string? CategoriaNome,
    DateTime? DataLimite,
    DateTime? DataConclusao,
    DateTime DataCriacao,
    DateTime? DataAtualizacao,
    int QuantidadeComentarios,
    int QuantidadeAnexos
);
