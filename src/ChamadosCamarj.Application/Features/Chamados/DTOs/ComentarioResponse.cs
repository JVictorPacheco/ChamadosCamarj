using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.DTOs;

public record ComentarioResponse(
    Guid Id,
    string Autor,
    string Conteudo,
    TipoComentario Tipo,
    DateTime DataCriacao
);
