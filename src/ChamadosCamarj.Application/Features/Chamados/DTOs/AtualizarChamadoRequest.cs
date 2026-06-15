using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Chamados.DTOs;

public record AtualizarChamadoRequest(
    string Titulo,
    string Descricao
);
