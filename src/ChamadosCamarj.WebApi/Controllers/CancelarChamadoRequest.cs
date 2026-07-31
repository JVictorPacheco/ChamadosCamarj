namespace ChamadosCamarj.WebApi.Controllers;

public record CancelarChamadoRequest(
    string Motivo,
    string? MotivoOutro = null,
    string? Observacao = null);