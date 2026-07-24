using MediatR;
using Microsoft.AspNetCore.Identity;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Usuarios.Commands;

public class CriarUsuarioPerfilCommandHandler : IRequestHandler<CriarUsuarioPerfilCommand, UsuarioPerfilResponse>
{
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IPasswordHasher<UsuarioPerfil> _passwordHasher;

    public CriarUsuarioPerfilCommandHandler(
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IPasswordHasher<UsuarioPerfil> passwordHasher)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UsuarioPerfilResponse> Handle(CriarUsuarioPerfilCommand request, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var emailNormalizado = request.Email.Trim().ToLowerInvariant();
        var existente = await _usuarioPerfilRepository.ObterPorEmailAsync(emailNormalizado, cancellationToken);
        if (existente is not null)
        {
            if (existente.Ativo)
                throw new ConflictException($"Já existe um usuário ativo com o e-mail '{emailNormalizado}'.");

            // E-mail pertence a um usuário desativado: reativa o registro existente em vez de
            // inserir um novo, já que o índice único de Email não distingue ativo/inativo.
            existente.Atualizar(request.Nome, request.Perfil);
            existente.DefinirSenhaHash(_passwordHasher.HashPassword(existente, request.Senha));
            existente.Ativar();
            await _usuarioPerfilRepository.AtualizarAsync(existente, cancellationToken);
            return existente.ToResponse();
        }

        var usuario = new UsuarioPerfil(request.Email, request.Nome, request.Perfil);
        usuario.DefinirSenhaHash(_passwordHasher.HashPassword(usuario, request.Senha));

        await _usuarioPerfilRepository.AdicionarAsync(usuario, cancellationToken);

        return usuario.ToResponse();
    }
}
