namespace ChamadosCamarj.Application.Features.Chamados.DTOs;

public record ReatribuirRequest(
    Guid NovoResponsavelId,
    string NovoResponsavelNome,
    Guid? UsuarioId = null,
    string UsuarioNome = "Sistema"
);
