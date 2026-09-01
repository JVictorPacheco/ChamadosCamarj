namespace ChamadosCamarj.Application.Features.Chat.DTOs;

public record ChatArquivoResponse(
    string NomeArquivo,
    string UrlAssinada,
    string TipoArquivo,
    long TamanhoBytes
);
