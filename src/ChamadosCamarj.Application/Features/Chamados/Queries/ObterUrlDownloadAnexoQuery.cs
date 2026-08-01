using MediatR;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public record ObterUrlDownloadAnexoQuery(Guid AnexoId) : IRequest<string>;
