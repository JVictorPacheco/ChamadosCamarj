using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Common.Autorizacao;
using MediatR;
using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chat.Commands.DefinirChatPerfil;
using ChamadosCamarj.Application.Features.Usuarios.DTOs;
using ChamadosCamarj.Application.Mappings;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Usuarios.Commands;

public class AtualizarUsuarioPerfilCommandHandler : IRequestHandler<AtualizarUsuarioPerfilCommand, UsuarioPerfilResponse?>
{
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IMediator _mediator;
    private readonly IAuditoriaAcessoRepository _auditoriaAcesso;

    public AtualizarUsuarioPerfilCommandHandler(
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IMediator mediator,
        IAuditoriaAcessoRepository auditoriaAcesso)
    {
        _auditoriaAcesso = auditoriaAcesso;
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _mediator = mediator;
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

        var ativoAnterior = usuario.Ativo;
        var grupoAnterior = usuario.GrupoId;

        if (request.Ativo && !usuario.Ativo)
            usuario.Ativar();
        else if (!request.Ativo && usuario.Ativo)
            usuario.Desativar();

        var perfilAnterior = usuario.Perfil;
        usuario.Atualizar(request.Nome, request.Perfil, request.GrupoId);

        // spec controle-de-acesso AC-15: mudou o perfil, os ajustes de módulos (feitos para o perfil antigo)
        // são apagados e a pessoa passa ao padrão do novo perfil. O Chat é mantido.
        var perfilMudou = perfilAnterior != request.Perfil;
        if (perfilMudou)
            usuario.VoltarAoPadraoDeModulos();

        await _usuarioPerfilRepository.AtualizarAsync(usuario, cancellationToken);

        // A review independente (review-fase8-independente.md #1) pegou essa tela duplicando a
        // auditoria/notificação de ChatPerfil do DefinirChatPerfilCommandHandler por cópia literal —
        // e a Fase 9, feita depois, só evoluiu a cópia original, deixando os dois caminhos divergirem
        // de novo (review-fase9-independente.md #1: AC-46/47/48 não valiam aqui). Em vez de manter
        // duas cópias sincronizadas à mão, despacha o mesmo comando: um único lugar decide o que
        // acontece quando o ChatPerfil de alguém muda, não importa por qual tela.
        if (request.ChatPerfil is { } novoChat && novoChat != usuario.ChatPerfil)
        {
            await _mediator.Send(
                new DefinirChatPerfilCommand(usuario.Id, novoChat, request.PerfilRequisitante ?? "", request.RequisitanteId, request.RequisitanteNome),
                cancellationToken);

            // DefinirChatPerfilCommandHandler persiste a mudança lendo/salvando sua própria cópia
            // (repositório usa AsNoTracking) — sem isso, a resposta desta chamada devolveria o
            // ChatPerfil antigo, mesmo já correto no banco.
            usuario.DefinirChatPerfil(novoChat);
        }

        if (perfilMudou)
        {
            await _auditoriaAcesso.AdicionarAsync(
                [AuditoriaAcesso.Criar(usuario.Id, usuario.Nome, request.RequisitanteId, request.RequisitanteNome,
                    "Perfil", perfilAnterior.ToString(), $"{request.Perfil} (ajustes zerados)")],
                cancellationToken);
            // O menu da pessoa muda junto com o perfil: avisa na hora (AC-11/AC-13).
            await _mediator.Publish(new AcessosAtualizadosNotification(
                usuario.Id, ModulosDeAcesso.Nomes(usuario.ModulosEfetivos()), usuario.ChatPerfil, usuario.Perfil), cancellationToken);
        }

        // spec perfil-no-cadastro: o servidor já usa o cadastro a cada pedido; o tempo real, que guarda a
        // conexão aberta, precisa ser avisado para reajustar os grupos de avisos e derrubar conta desativada.
        if (perfilMudou || usuario.Ativo != ativoAnterior || usuario.GrupoId != grupoAnterior)
        {
            await _mediator.Publish(new CadastroDeAcessoAlteradoNotification(
                usuario.Id, usuario.Perfil, usuario.Ativo, usuario.GrupoId), cancellationToken);
        }

        return usuario.ToResponse();
    }
}
