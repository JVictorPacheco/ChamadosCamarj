namespace ChamadosCamarj.Application.Features.Chamados.DTOs;

public record AnexoResponse(
    Guid Id,
    string NomeArquivo,
    string TipoArquivo,
    long TamanhoBytes,
    Guid? EnviadoPorId,
    string EnviadoPorNome,
    DateTime DataCriacao
);
