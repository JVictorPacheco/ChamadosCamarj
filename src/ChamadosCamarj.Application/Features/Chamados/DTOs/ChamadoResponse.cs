using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.DTOs;

public record ChamadoResponse(
    Guid Id,
    int Numero,
    string Titulo,
    string Descricao,
    StatusChamado Status,
    PrioridadeChamado Prioridade,
    string SolicitanteNome,
    string SolicitanteEmail,
    Guid? ResponsavelId,
    string? ResponsavelNome,
    Guid? AreaId,
    string? AreaNome,
    Guid? TipoId,
    string? TipoNome,
    DateTime? DataLimite,
    DateTime? DataConclusao,
    DateTime DataCriacao,
    DateTime? DataAtualizacao,
    int QuantidadeComentarios,
    int QuantidadeAnexos,
    SlaStatus SlaStatus,
    string SlaLabel,
    double? SlaHorasRestantes,
    // Versão para a checagem de edição simultânea (spec correcoes-pre-deploy AC-09) — texto opaco.
    string Versao,
    Domain.Enums.MotivoEncerramento? MotivoEncerramento = null,
    string? MotivoOutro = null
);
