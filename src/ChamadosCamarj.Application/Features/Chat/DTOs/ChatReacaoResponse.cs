namespace ChamadosCamarj.Application.Features.Chat.DTOs;

public record ChatReacaoResponse(
    string Emoji,
    int Quantidade,
    bool ReagiuEu
);
