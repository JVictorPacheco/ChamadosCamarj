using MediatR;
using ChamadosCamarj.Application.Features.Chamados.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Chamados.Commands;

public class AbrirChamadoCommandHandler : IRequestHandler<AbrirChamadoCommand, ChamadoResponse>
{
    private readonly IChamadoRepository _chamadoRepository;

    public AbrirChamadoCommandHandler(IChamadoRepository chamadoRepository)
    {
        _chamadoRepository = chamadoRepository;
    }

    public async Task<ChamadoResponse> Handle(AbrirChamadoCommand request, CancellationToken cancellationToken)
    {
        var chamado = new Chamado(
            request.Titulo,
            request.Descricao,
            request.SolicitanteNome,
            request.SolicitanteEmail,
            request.CategoriaId,
            request.Prioridade
        );

        await _chamadoRepository.AdicionarAsync(chamado, cancellationToken);

        return chamado.ToResponse();
    }
}
