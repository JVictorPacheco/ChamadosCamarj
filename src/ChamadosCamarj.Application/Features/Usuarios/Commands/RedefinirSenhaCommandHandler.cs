using MediatR;
using Microsoft.AspNetCore.Identity;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Usuarios.Commands;

public class RedefinirSenhaCommandHandler : IRequestHandler<RedefinirSenhaCommand>
{
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IPasswordHasher<UsuarioPerfil> _passwordHasher;

    public RedefinirSenhaCommandHandler(
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IPasswordHasher<UsuarioPerfil> passwordHasher)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(RedefinirSenhaCommand request, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var usuario = await _usuarioPerfilRepository.ObterPorIdAsync(request.Id, cancellationToken);
        if (usuario is null)
            throw new NotFoundException("Usuário", request.Id);

        usuario.DefinirSenhaHash(_passwordHasher.HashPassword(usuario, request.NovaSenha));
        await _usuarioPerfilRepository.AtualizarAsync(usuario, cancellationToken);
    }
}
