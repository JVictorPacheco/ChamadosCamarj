using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Infrastructure.Data;
using ChamadosCamarj.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ChamadosCamarj.UnitTests.Infrastructure;

/// <summary>
/// Change tracker real do EF (não Moq). Salvar acessos grava o mesmo usuário duas vezes na mesma requisição
/// (módulos + DefinirChatPerfilCommand), cada gravação com a sua cópia lida do banco; com Área (Grupo), a
/// segunda dava InvalidOperationException → 500 (review-3 R-01 de controle-de-acesso).
/// </summary>
public class UsuarioPerfilRepositoryTests
{
    private static ApplicationDbContext NovoContexto(string banco) =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(banco).Options);

    private static async Task<Guid> SemearUsuarioComGrupoAsync(string banco)
    {
        await using var contexto = NovoContexto(banco);
        var grupo = new Grupo("Faturamento", "Área de teste");
        contexto.Add(grupo);
        await contexto.SaveChangesAsync();
        var usuario = new UsuarioPerfil("pessoa@camarj.com.br", "Pessoa", Perfil.Atendente);
        usuario.DefinirGrupo(grupo.Id);
        contexto.Add(usuario);
        await contexto.SaveChangesAsync();
        return usuario.Id;
    }

    [Fact]
    public async Task AtualizarAsync_DuasCopiasDoMesmoUsuarioComGrupoNaMesmaRequisicao_GravaAsDuas()
    {
        var banco = Guid.NewGuid().ToString();
        var id = await SemearUsuarioComGrupoAsync(banco);
        await using var contexto = NovoContexto(banco); // um contexto por requisição, como no DI
        var repositorio = new UsuarioPerfilRepository(contexto);

        // 1ª gravação: módulos (SalvarAcessosCommandHandler).
        var primeira = (await repositorio.ObterPorIdAsync(id, CancellationToken.None))!;
        primeira.AjustarModulos(ModuloSistema.Nenhum, ModuloSistema.RelatorioMensal);
        await repositorio.AtualizarAsync(primeira, CancellationToken.None);

        // 2ª gravação: Chat (DefinirChatPerfilCommandHandler lê a própria cópia).
        var segunda = (await repositorio.ObterPorIdAsync(id, CancellationToken.None))!;
        segunda.DefinirChatPerfil(ChatPerfil.Participante);
        var gravar = () => repositorio.AtualizarAsync(segunda, CancellationToken.None);

        await gravar.Should().NotThrowAsync();
        await using var conferencia = NovoContexto(banco);
        var gravado = await conferencia.Set<UsuarioPerfil>().AsNoTracking().SingleAsync(u => u.Id == id);
        gravado.ModulosRetirados.Should().Be(ModuloSistema.RelatorioMensal);
        gravado.ChatPerfil.Should().Be(ChatPerfil.Participante);
        gravado.GrupoId.Should().NotBeNull();
    }

    // Consumidor fora do escopo (Editar usuário): trocar a Área com a Área antiga carregada pelo Include
    // grava a nova, e tirar a Área grava sem Área.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AtualizarAsync_TrocarOuTirarArea_ComAreaAntigaCarregada_GravaONovoValor(bool trocar)
    {
        var banco = Guid.NewGuid().ToString();
        var id = await SemearUsuarioComGrupoAsync(banco);
        Guid? novaArea = null;
        if (trocar)
        {
            await using var semente = NovoContexto(banco);
            var outra = new Grupo("Contas Médicas", "Outra área");
            semente.Add(outra);
            await semente.SaveChangesAsync();
            novaArea = outra.Id;
        }

        await using var contexto = NovoContexto(banco);
        var repositorio = new UsuarioPerfilRepository(contexto);
        var usuario = (await repositorio.ObterPorIdAsync(id, CancellationToken.None))!;
        usuario.Grupo.Should().NotBeNull();
        usuario.Atualizar("Pessoa", Perfil.Atendente, novaArea);
        await repositorio.AtualizarAsync(usuario, CancellationToken.None);

        await using var conferencia = NovoContexto(banco);
        (await conferencia.Set<UsuarioPerfil>().AsNoTracking().SingleAsync(u => u.Id == id)).GrupoId.Should().Be(novaArea);
        (await conferencia.Set<Grupo>().CountAsync()).Should().Be(trocar ? 2 : 1); // a Área não é recriada nem apagada
    }
}
