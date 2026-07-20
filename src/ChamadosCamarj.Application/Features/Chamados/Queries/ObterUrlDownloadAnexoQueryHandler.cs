using MediatR;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Queries;

public class ObterUrlDownloadAnexoQueryHandler : IRequestHandler<ObterUrlDownloadAnexoQuery, string>
{
    private const int ExpiracaoSegundos = 3600; // 1 hora

    private readonly IChamadoRepository _chamadoRepository;
    private readonly IStorageService _storageService;

    public ObterUrlDownloadAnexoQueryHandler(IChamadoRepository chamadoRepository, IStorageService storageService)
    {
        _chamadoRepository = chamadoRepository;
        _storageService = storageService;
    }

    public async Task<string> Handle(ObterUrlDownloadAnexoQuery request, CancellationToken cancellationToken)
    {
        var anexo = await _chamadoRepository.ObterAnexoPorIdAsync(request.AnexoId, cancellationToken)
            ?? throw new NotFoundException("Anexo", request.AnexoId);

        return await _storageService.ObterUrlAssinadaAsync(anexo.CaminhoStorage, ExpiracaoSegundos, cancellationToken);
    }
}
