using MediatR;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Usuarios.Queries;

public class ObterUsuarioPerfilPorEmailQueryHandler : IRequestHandler<ObterUsuarioPerfilPorEmailQuery, UsuarioPerfilResponse?>
{
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;

    public ObterUsuarioPerfilPorEmailQueryHandler(IUsuarioPerfilRepository usuarioPerfilRepository)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
    }

    public async Task<UsuarioPerfilResponse?> Handle(ObterUsuarioPerfilPorEmailQuery request, CancellationToken cancellationToken)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();
        var usuario = await _usuarioPerfilRepository.ObterPorEmailAsync(emailNormalizado, cancellationToken);

        if (usuario is null || !usuario.Ativo)
            return null;

        return usuario.ToResponse();
    }
}
