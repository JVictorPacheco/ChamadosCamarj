using ChamadosCamarj.Application.Common.Authorization;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Acessos.DTOs;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using MediatR;

namespace ChamadosCamarj.Application.Features.Acessos.Queries;

/// <summary>Lista do Controle de acesso — todos os usuários, Admins como "acesso total" (AC-01, AC-06).</summary>
public record ListarAcessosQuery(string PerfilRequisitante = "") : IRequest<IReadOnlyList<AcessoUsuarioResumoResponse>>;

public class ListarAcessosQueryHandler : IRequestHandler<ListarAcessosQuery, IReadOnlyList<AcessoUsuarioResumoResponse>>
{
    private readonly IUsuarioPerfilRepository _usuarios;

    public ListarAcessosQueryHandler(IUsuarioPerfilRepository usuarios) => _usuarios = usuarios;

    public async Task<IReadOnlyList<AcessoUsuarioResumoResponse>> Handle(ListarAcessosQuery request, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var usuarios = await _usuarios.ListarAsync(cancellationToken);
        return usuarios
            .OrderBy(u => u.Nome)
            .Select(u => new AcessoUsuarioResumoResponse(
                u.Id, u.Nome, u.Email, u.Perfil, u.Ativo,
                AcessoTotal: u.Perfil == Perfil.Admin,
                ModulosDeAcesso.Nomes(u.ModulosEfetivos()),
                u.ChatPerfil,
                TemAjuste: u.Perfil != Perfil.Admin && u.TemAjusteDeModulos))
            .ToList();
    }
}

/// <summary>Painel de uma pessoa: módulos (padrão × efetivo × ajustável), Chat e auditoria (AC-02, AC-14).</summary>
public record ObterAcessoUsuarioQuery(Guid UsuarioId, string PerfilRequisitante = "") : IRequest<AcessoUsuarioDetalheResponse>;

public class ObterAcessoUsuarioQueryHandler : IRequestHandler<ObterAcessoUsuarioQuery, AcessoUsuarioDetalheResponse>
{
    private readonly IUsuarioPerfilRepository _usuarios;
    private readonly IAuditoriaAcessoRepository _auditoria;

    public ObterAcessoUsuarioQueryHandler(IUsuarioPerfilRepository usuarios, IAuditoriaAcessoRepository auditoria)
    {
        _usuarios = usuarios;
        _auditoria = auditoria;
    }

    public async Task<AcessoUsuarioDetalheResponse> Handle(ObterAcessoUsuarioQuery request, CancellationToken cancellationToken)
    {
        PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante);

        var usuario = await _usuarios.ObterPorIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new NotFoundException("Usuário", request.UsuarioId);

        var padrao = ModulosDeAcesso.Padrao(usuario.Perfil);
        var ajustaveis = ModulosDeAcesso.Ajustaveis(usuario.Perfil);
        var efetivos = usuario.ModulosEfetivos();

        // Só os módulos que fazem sentido para o perfil: Kanban/Fila nem aparecem para Solicitante (AC-08).
        var modulos = Enum.GetValues<ModuloSistema>()
            .Where(m => m is not (ModuloSistema.Nenhum or ModuloSistema.Todos) && (padrao | ajustaveis).HasFlag(m))
            .Select(m => new ModuloAcessoItem(m.ToString(), padrao.HasFlag(m), efetivos.HasFlag(m), ajustaveis.HasFlag(m)))
            .ToList();

        var auditoria = (await _auditoria.ListarPorUsuarioAsync(usuario.Id, cancellationToken))
            .Select(a => new AuditoriaAcessoResponse(a.DataCriacao, a.AlteradoPorNome, a.Item, a.Anterior, a.Novo))
            .ToList();

        return new AcessoUsuarioDetalheResponse(
            usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil,
            AcessoTotal: usuario.Perfil == Perfil.Admin,
            modulos, usuario.ChatPerfil, auditoria);
    }
}
