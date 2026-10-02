using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        var gruposSeed = new Dictionary<Guid, (string Nome, string Descricao)>
        {
            [Guid.Parse("b1000000-0000-0000-0000-000000000001")] = ("Reembolso", "Equipe de reembolso"),
            [Guid.Parse("b1000000-0000-0000-0000-000000000002")] = ("Credenciado", "Equipe de credenciado"),
            [Guid.Parse("b1000000-0000-0000-0000-000000000003")] = ("Comercial", "Equipe comercial"),
            [Guid.Parse("b1000000-0000-0000-0000-000000000004")] = ("Contas Médicas", "Equipe de contas médicas"),
            [Guid.Parse("b1000000-0000-0000-0000-000000000005")] = ("Autorização/Auditoria", "Equipe de autorização e auditoria"),
            [Guid.Parse("b1000000-0000-0000-0000-000000000006")] = ("Atendimento", "Equipe de atendimento"),
        };

        // Grupos (= áreas) e tipos de chamado: só cria o que ainda não existe, nem pelo id nem pelo
        // nome. Nunca sobrescreve — antes o seeder desfazia a cada subida o que o Admin tinha editado
        // (spec area-e-tipo-do-chamado AC-10).
        gruposSeed[AreaETipoPadrao.AreaFinanceiro] = ("Financeiro", "Área financeira");
        gruposSeed[AreaETipoPadrao.AreaSuperETendencia] = ("Super e Tendência", "Supervisão e tendências");

        var nomesGrupos = await db.Grupos.Select(g => g.Nome).ToListAsync();
        var idsGrupos = await db.Grupos.Select(g => g.Id).ToListAsync();
        foreach (var (id, (nome, descricao)) in gruposSeed)
        {
            if (!idsGrupos.Contains(id) && !nomesGrupos.Contains(nome))
                db.Grupos.Add(new Grupo(nome, descricao) { Id = id });
        }

        var nomesTipos = await db.TiposChamado.Select(t => t.Nome).ToListAsync();
        var idsTipos = await db.TiposChamado.Select(t => t.Id).ToListAsync();
        foreach (var (id, nome, descricao, ativo) in AreaETipoPadrao.Tipos)
        {
            if (idsTipos.Contains(id) || nomesTipos.Contains(nome))
                continue;
            var tipo = new TipoChamado(nome, descricao) { Id = id };
            if (!ativo)
                tipo.Desativar();
            db.TiposChamado.Add(tipo);
        }

        await db.SaveChangesAsync();

        // Chamados sem área/tipo (anteriores à mudança, ou abertos pela versão anterior durante o
        // deploy) recebem área = grupo da categoria e tipo = "Não classificado". Idempotente.
        await db.Database.ExecuteSqlRawAsync(AreaETipoPadrao.SqlPreencherAreaETipo());

        if (!await db.UsuariosPerfil.AnyAsync())
        {
            var usuarios = new List<UsuarioPerfil>
            {
                new UsuarioPerfil("victor@camarj.com.br", "Victor", Perfil.Admin)     { Id = Guid.Parse("a1000000-0000-0000-0000-000000000001") },
            };

            var fabioUsuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente) { Id = Guid.Parse("a2000000-0000-0000-0000-000000000002") };
            fabioUsuario.DefinirGrupo(Guid.Parse("b1000000-0000-0000-0000-000000000001"));
            usuarios.Add(fabioUsuario);

            db.UsuariosPerfil.AddRange(usuarios);
            await db.SaveChangesAsync();
        }
    }
}
