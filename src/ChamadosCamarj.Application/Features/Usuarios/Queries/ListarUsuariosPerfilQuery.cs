using MediatR;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;

namespace ChamadosCamarj.Application.Features.Usuarios.Queries;

public record ListarUsuariosPerfilQuery(string? PerfilRequisitante = null) : IRequest<IEnumerable<UsuarioPerfilResponse>>;
