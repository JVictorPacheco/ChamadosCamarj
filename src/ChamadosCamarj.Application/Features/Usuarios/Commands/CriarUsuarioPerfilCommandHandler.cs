using MediatR;
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

    public CriarUsuarioPerfilCommandHandler(IUsuarioPerfilRepository usuarioPerfilRepository)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
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
            existente.Ativar();
            await _usuarioPerfilRepository.AtualizarAsync(existente, cancellationToken);
            return existente.ToResponse();
        }

        var usuario = new UsuarioPerfil(request.Email, request.Nome, request.Perfil);

        await _usuarioPerfilRepository.AdicionarAsync(usuario, cancellationToken);

        return usuario.ToResponse();
    }
}
