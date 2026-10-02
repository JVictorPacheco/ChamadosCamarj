using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.Commands.DefinirChatPerfil;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using MediatR;

namespace ChamadosCamarj.Application.Features.Acessos.Commands;

/// <summary>
/// Admin define os módulos e o nível do Chat de uma pessoa (spec controle-de-acesso AC-03, AC-07, AC-09).
/// <paramref name="Modulos"/> = os módulos que a pessoa deve ter, marcados na tela.
/// </summary>
public record SalvarAcessosCommand(
    Guid UsuarioId,
    IReadOnlyList<string> Modulos,
    ChatPerfil ChatPerfil,
    string PerfilRequisitante = "",
    Guid AdminId = default,
    string AdminNome = "Sistema"
) : IRequest;

public class SalvarAcessosCommandHandler : IRequestHandler<SalvarAcessosCommand>
{
    private readonly IUsuarioPerfilRepository _usuarios;
    private readonly IAuditoriaAcessoRepository _auditoria;
    private readonly IMediator _mediator;

    public SalvarAcessosCommandHandler(IUsuarioPerfilRepository usuarios, IAuditoriaAcessoRepository auditoria, IMediator mediator)
    {
        _usuarios = usuarios;
        _auditoria = auditoria;
        _mediator = mediator;
    }

    public async Task Handle(SalvarAcessosCommand request, CancellationToken cancellationToken)
    {
        var usuario = await AcessosGuard.ObterAlvoAsync(_usuarios, request.UsuarioId, request.PerfilRequisitante, request.AdminId, cancellationToken);

        var desejados = ModulosDeAcesso.DeNomes(request.Modulos);
        var (concedidos, retirados) = ModulosDeAcesso.CalcularAjustes(usuario.Perfil, desejados);

        var antes = usuario.ModulosEfetivos();
        var registros = new List<AuditoriaAcesso>();

        if (concedidos != usuario.ModulosConcedidos || retirados != usuario.ModulosRetirados)
        {
            usuario.AjustarModulos(concedidos, retirados);
            await _usuarios.AtualizarAsync(usuario, cancellationToken);
            registros.AddRange(AcessosTexto.DiferencasDeModulos(usuario, antes, usuario.ModulosEfetivos(), request.AdminId, request.AdminNome));
        }

        var chatAntes = usuario.ChatPerfil;
        if (request.ChatPerfil != chatAntes)
        {
            // Mesmo comando de antes da feature: mantém os avisos aos participantes e a auditoria do chat (AC-09).
            await _mediator.Send(new DefinirChatPerfilCommand(usuario.Id, request.ChatPerfil, request.PerfilRequisitante, request.AdminId, request.AdminNome), cancellationToken);
            registros.Add(AuditoriaAcesso.Criar(usuario.Id, usuario.Nome, request.AdminId, request.AdminNome,
                "Chat", AcessosTexto.Chat(chatAntes), AcessosTexto.Chat(request.ChatPerfil)));
        }

        if (registros.Count == 0)
            return;

        await _auditoria.AdicionarAsync(registros, cancellationToken);
        await _mediator.Publish(new AcessosAtualizadosNotification(
            usuario.Id, ModulosDeAcesso.Nomes(usuario.ModulosEfetivos()), request.ChatPerfil), cancellationToken);
    }
}

/// <summary>Volta a pessoa ao padrão de módulos do perfil; o Chat não muda (AC-04).</summary>
public record VoltarAoPadraoCommand(
    Guid UsuarioId,
    string PerfilRequisitante = "",
    Guid AdminId = default,
    string AdminNome = "Sistema"
) : IRequest;

public class VoltarAoPadraoCommandHandler : IRequestHandler<VoltarAoPadraoCommand>
{
    private readonly IUsuarioPerfilRepository _usuarios;
    private readonly IAuditoriaAcessoRepository _auditoria;
    private readonly IPublisher _publisher;

    public VoltarAoPadraoCommandHandler(IUsuarioPerfilRepository usuarios, IAuditoriaAcessoRepository auditoria, IPublisher publisher)
    {
        _usuarios = usuarios;
        _auditoria = auditoria;
        _publisher = publisher;
    }

    public async Task Handle(VoltarAoPadraoCommand request, CancellationToken cancellationToken)
    {
        var usuario = await AcessosGuard.ObterAlvoAsync(_usuarios, request.UsuarioId, request.PerfilRequisitante, request.AdminId, cancellationToken);
        if (!usuario.TemAjusteDeModulos)
            return;

        var antes = usuario.ModulosEfetivos();
        usuario.VoltarAoPadraoDeModulos();
        await _usuarios.AtualizarAsync(usuario, cancellationToken);

        await _auditoria.AdicionarAsync(
            AcessosTexto.DiferencasDeModulos(usuario, antes, usuario.ModulosEfetivos(), request.AdminId, request.AdminNome),
            cancellationToken);
        await _publisher.Publish(new AcessosAtualizadosNotification(
            usuario.Id, ModulosDeAcesso.Nomes(usuario.ModulosEfetivos()), usuario.ChatPerfil), cancellationToken);
    }
}

/// <summary>Regras comuns de quem pode mexer em acessos de quem (AC-05, AC-06).</summary>
internal static class AcessosGuard
{
    public static async Task<UsuarioPerfil> ObterAlvoAsync(
        IUsuarioPerfilRepository usuarios, Guid usuarioId, string perfilRequisitante, Guid adminId, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(perfilRequisitante);

        if (usuarioId == adminId)
            throw new BadRequestException("Você não pode alterar o próprio acesso.");

        var usuario = await usuarios.ObterPorIdAsync(usuarioId, cancellationToken)
            ?? throw new NotFoundException("Usuário", usuarioId);

        if (usuario.Perfil == Perfil.Admin)
            throw new BadRequestException("O acesso do Admin é total e não pode ser ajustado.");

        return usuario;
    }
}
