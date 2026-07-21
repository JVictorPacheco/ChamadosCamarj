using MediatR;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.DTOs;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class AdicionarAnexoCommandHandler : IRequestHandler<AdicionarAnexoCommand, AnexoResponse>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly IStorageService _storageService;

    public AdicionarAnexoCommandHandler(IChamadoRepository chamadoRepository, IStorageService storageService)
    {
        _chamadoRepository = chamadoRepository;
        _storageService = storageService;
    }

    public async Task<AnexoResponse> Handle(AdicionarAnexoCommand request, CancellationToken cancellationToken)
    {
        var existe = await _chamadoRepository.ExisteAsync(request.ChamadoId, cancellationToken);
        if (!existe)
            throw new NotFoundException("Chamado", request.ChamadoId);

        var extensao = Path.GetExtension(request.NomeArquivoOriginal);
        var caminho = $"{request.ChamadoId}/{Guid.NewGuid()}{extensao}";

        await _storageService.UploadAsync(caminho, request.ContentType, request.Conteudo, cancellationToken);

        var anexo = new Anexo(
            request.ChamadoId,
            request.NomeArquivoOriginal,
            caminho,
            request.ContentType,
            request.TamanhoBytes,
            request.UsuarioId,
            request.UsuarioNome,
            request.ComentarioId
        );

        await _chamadoRepository.AdicionarAnexoAsync(anexo, cancellationToken);

        return new AnexoResponse(anexo.Id, anexo.NomeArquivo, anexo.TipoArquivo, anexo.TamanhoBytes, anexo.EnviadoPorNome, anexo.DataCriacao);
    }
}
