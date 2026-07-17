using MediatR;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Usuarios.Queries;

public class ListarUsuariosPerfilQueryHandler : IRequestHandler<ListarUsuariosPerfilQuery, IEnumerable<UsuarioPerfilResponse>>
{
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;

    public ListarUsuariosPerfilQueryHandler(IUsuarioPerfilRepository usuarioPerfilRepository)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
    }

    public async Task<IEnumerable<UsuarioPerfilResponse>> Handle(ListarUsuariosPerfilQuery request, CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioPerfilRepository.ListarAsync(cancellationToken);

        return usuarios
            .OrderBy(u => u.Nome)
            .Select(u => u.ToResponse());
    }
}
