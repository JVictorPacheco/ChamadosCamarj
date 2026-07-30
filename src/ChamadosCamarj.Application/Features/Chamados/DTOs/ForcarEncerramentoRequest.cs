namespace ChamadosCamarj.Application.Features.Chamados.DTOs;

public record ForcarEncerramentoRequest(
    string Motivo,
    string? MotivoOutro = null);
