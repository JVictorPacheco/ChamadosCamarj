using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Enums;
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
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var usuario = await _usuarioPerfilRepository.ObterPorIdAsync(request.Id, cancellationToken);
        if (usuario is null)
            return null;

        var eraAdminAtivo = usuario.Perfil == Perfil.Admin && usuario.Ativo;
        var deixaDeSerAdminAtivo = eraAdminAtivo && (!request.Ativo || request.Perfil != Perfil.Admin);

        if (deixaDeSerAdminAtivo)
        {
            var usuarios = await _usuarioPerfilRepository.ListarAsync(cancellationToken);
            var totalAdminsAtivos = usuarios.Count(u => u.Perfil == Perfil.Admin && u.Ativo);

            if (totalAdminsAtivos <= 1)
                throw new ConflictException("Não é possível desativar/rebaixar o último Admin ativo do sistema.");
        }

        if (request.Ativo && !usuario.Ativo)
            usuario.Ativar();
        else if (!request.Ativo && usuario.Ativo)
            usuario.Desativar();

        usuario.Atualizar(request.Nome, request.Perfil, request.GrupoId);

        await _usuarioPerfilRepository.AtualizarAsync(usuario, cancellationToken);

        return usuario.ToResponse();
    }
}
