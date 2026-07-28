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

        var houveMudancaGrupos = false;

        foreach (var (id, (nome, descricao)) in gruposSeed)
        {
            var existente = await db.Grupos.FindAsync(id);
            if (existente == null)
            {
                db.Grupos.Add(new Grupo(nome, descricao) { Id = id });
                houveMudancaGrupos = true;
            }
            else if (existente.Nome != nome)
            {
                existente.Atualizar(nome, descricao);
                houveMudancaGrupos = true;
            }
        }

        if (houveMudancaGrupos)
            await db.SaveChangesAsync();

        var categoriasSeed = new Dictionary<Guid, (string Nome, string Descricao)>
        {
            [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567891")] = ("Autorização/Auditoria", "Pedidos de autorização e auditoria"),
            [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567892")] = ("Atendimento", "Atendimento geral"),
            [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567893")] = ("Super e Tendência", "Assuntos de supervisão e tendências"),
            [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567894")] = ("Reembolso", "Solicitações de reembolso"),
            [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567895")] = ("Financeiro", "Assuntos financeiros"),
            [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567896")] = ("Credenciado", "Assuntos de credenciamento"),
            [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567897")] = ("Comercial", "Assuntos comerciais"),
            [Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567898")] = ("Contas Médicas", "Assuntos de contas médicas"),
        };

        var houveMudanca = false;

        foreach (var (id, (nome, descricao)) in categoriasSeed)
        {
            var existente = await db.Categorias.FindAsync(id);
            if (existente == null)
            {
                db.Categorias.Add(new Categoria(nome, descricao) { Id = id });
                houveMudanca = true;
            }
            else if (existente.Nome != nome)
            {
                existente.Atualizar(nome, descricao);
                houveMudanca = true;
            }
        }

        if (houveMudanca)
            await db.SaveChangesAsync();

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
