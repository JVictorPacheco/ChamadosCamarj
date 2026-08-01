using MediatR;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class RemoverAnexoCommandHandler : IRequestHandler<RemoverAnexoCommand>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IStorageService _storageService;

    public RemoverAnexoCommandHandler(IChamadoRepository chamadoRepository, IStorageService storageService)
    {
        _chamadoRepository = chamadoRepository;
        _storageService = storageService;
    }

    private const string PerfilAdmin = "Admin";

    public async Task Handle(RemoverAnexoCommand request, CancellationToken cancellationToken)
    {
        var anexo = await _chamadoRepository.ObterAnexoPorIdAsync(request.AnexoId, cancellationToken);
        if (anexo is null || anexo.ChamadoId != request.ChamadoId)
            throw new NotFoundException("Anexo", request.AnexoId);

        var isAdmin = string.Equals(request.PerfilRequisitante, PerfilAdmin, StringComparison.OrdinalIgnoreCase);
        if (!isAdmin && anexo.EnviadoPorId != request.RequisitanteId)
            throw new ForbiddenException("Você só pode remover anexos que você mesmo enviou.");

        await _storageService.RemoverAsync(anexo.CaminhoStorage, cancellationToken);
        await _chamadoRepository.RemoverAnexoAsync(anexo.Id, cancellationToken);
    }
}
