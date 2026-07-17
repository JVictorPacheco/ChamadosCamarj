using MediatR;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;

namespace ChamadosCamarj.Application.Features.Usuarios.Queries;

public record ListarUsuariosPerfilQuery : IRequest<IEnumerable<UsuarioPerfilResponse>>;
