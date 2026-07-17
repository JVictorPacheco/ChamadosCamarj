using MediatR;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Usuarios.Commands;

public class AtualizarUsuarioPerfilCommandHandler : IRequestHandler<AtualizarUsuarioPerfilCommand, UsuarioPerfilResponse?>
{
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;

    public AtualizarUsuarioPerfilCommandHandler(IUsuarioPerfilRepository usuarioPerfilRepository)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
    }

    public async Task<UsuarioPerfilResponse?> Handle(AtualizarUsuarioPerfilCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioPerfilRepository.ObterPorIdAsync(request.Id, cancellationToken);
        if (usuario is null)
            return null;

        if (request.Ativo && !usuario.Ativo)
            usuario.Ativar();
        else if (!request.Ativo && usuario.Ativo)
            usuario.Desativar();

        usuario.Atualizar(request.Nome, request.Perfil);

        await _usuarioPerfilRepository.AtualizarAsync(usuario, cancellationToken);

        return usuario.ToResponse();
    }
}
