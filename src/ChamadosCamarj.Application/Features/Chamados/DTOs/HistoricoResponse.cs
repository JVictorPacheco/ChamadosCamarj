using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.DTOs;

public record HistoricoResponse(
    Guid Id,
    Guid ChamadoId,
    string UsuarioNome,
    Guid? UsuarioId,
    AcaoHistorico Acao,
    string? DetalheAnterior,
    string? DetalheNovo,
    DateTime DataHora
);
