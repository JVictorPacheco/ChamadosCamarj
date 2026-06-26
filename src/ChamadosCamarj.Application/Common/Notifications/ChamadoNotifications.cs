using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;

namespace ChamadosCamarj.Application.Common.Notifications;

public record ChamadoCriadoNotification(Guid ChamadoId, string Titulo, StatusChamadoNotification Status) : INotification;

public record StatusAlteradoNotification(Guid ChamadoId, string NovoStatus, DateTime DataAtualizacao) : INotification;

public record ComentarioAdicionadoNotification(Guid ChamadoId, string Autor, string Conteudo) : INotification;

public record MetricasAtualizadasNotification : INotification;

/// <summary>
/// Versão simples do status para evitar dependência circular com o Domain.
/// </summary>
public enum StatusChamadoNotification
{
    Aberto,
    EmAndamento,
    Resolvido,
    Fechado,
    Cancelado
}
