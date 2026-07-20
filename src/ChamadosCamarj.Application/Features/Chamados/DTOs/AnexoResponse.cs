namespace ChamadosCamarj.Application.Features.Chamados.DTOs;

public record AnexoResponse(
    Guid Id,
    string NomeArquivo,
    string TipoArquivo,
    long TamanhoBytes,
    string EnviadoPorNome,
    DateTime DataCriacao
);
