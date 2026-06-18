namespace ChamadosCamarj.Application.Features.Chamados.DTOs;

public record ComentarChamadoRequest(
    string Autor,
    string Conteudo,
    bool Interno = false
);
