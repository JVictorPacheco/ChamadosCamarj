using MediatR;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Auth.Queries;

public class ObterPerfilAtualQueryHandler : IRequestHandler<ObterPerfilAtualQuery, UsuarioPerfilResponse>
{
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;

    public ObterPerfilAtualQueryHandler(IUsuarioPerfilRepository usuarioPerfilRepository)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
    }

    public async Task<UsuarioPerfilResponse> Handle(ObterPerfilAtualQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioPerfilRepository.ObterPorIdAsync(request.UsuarioId, cancellationToken);

        // Conta pode ter sido excluída/desativada depois do token ser emitido — trata como sessão
        // inválida (mesmo caminho de erro do login), não como "não encontrado" genérico, pra cair
        // no mesmo fluxo de logout automático (registrarLogoutAutomatico, em 401) do frontend.
        if (usuario is null || !usuario.Ativo)
            throw new UnauthorizedException("Sessão inválida.");

        return usuario.ToResponse();
    }
}
