using ChamadosCamarj.Application.Features.Acessos;
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
    private readonly IAuditoriaAcessoRepository _auditoriaAcesso;

    public CriarUsuarioPerfilCommandHandler(
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IPasswordHasher<UsuarioPerfil> passwordHasher,
        IAuditoriaAcessoRepository auditoriaAcesso)
    {
        _auditoriaAcesso = auditoriaAcesso;
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
            var perfilAnterior = existente.Perfil;
            var chatAnterior = existente.ChatPerfil;
            existente.Atualizar(request.Nome, request.Perfil, request.GrupoId);
            existente.DefinirSenhaHash(_passwordHasher.HashPassword(existente, request.Senha));
            existente.DefinirChatPerfil(request.ChatPerfil);
            // Recriar a conta é começar do zero: ajustes de módulos de antes não voltam sozinhos
            // (spec controle-de-acesso, review R-03).
            existente.VoltarAoPadraoDeModulos();
            existente.Ativar();
            await _usuarioPerfilRepository.AtualizarAsync(existente, cancellationToken);

            // Reativar muda perfil, módulos e Chat: entra na auditoria de acessos (review-2 R-04).
            var registros = new List<AuditoriaAcesso>
            {
                AuditoriaAcesso.Criar(existente.Id, existente.Nome, request.RequisitanteId, request.RequisitanteNome,
                    "Conta", $"desativada ({perfilAnterior})", $"reativada como {request.Perfil}"),
            };
            if (chatAnterior != request.ChatPerfil)
                registros.Add(AuditoriaAcesso.Criar(existente.Id, existente.Nome, request.RequisitanteId, request.RequisitanteNome,
                    "Chat", AcessosTexto.Chat(chatAnterior), AcessosTexto.Chat(request.ChatPerfil)));
            await _auditoriaAcesso.AdicionarAsync(registros, cancellationToken);

            return existente.ToResponse();
        }

        var usuario = new UsuarioPerfil(request.Email, request.Nome, request.Perfil);
        usuario.DefinirSenhaHash(_passwordHasher.HashPassword(usuario, request.Senha));
        usuario.DefinirChatPerfil(request.ChatPerfil);

        if (request.GrupoId.HasValue)
            usuario.DefinirGrupo(request.GrupoId.Value);

        await _usuarioPerfilRepository.AdicionarAsync(usuario, cancellationToken);

        return usuario.ToResponse();
    }
}
