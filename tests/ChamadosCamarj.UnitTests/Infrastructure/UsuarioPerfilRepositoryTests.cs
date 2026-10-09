using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Infrastructure.Data;
using ChamadosCamarj.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

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
    public async Task ObterIdentidadeAsync_UsuarioExistente_DevolvePerfilAtivoEEquipe()
    {
        var banco = Guid.NewGuid().ToString();
        var id = await SemearUsuarioComGrupoAsync(banco);
        await using var contexto = NovoContexto(banco);
        var repositorio = new UsuarioPerfilRepository(contexto, new MemoryCache(new MemoryCacheOptions()));

        var identidade = await repositorio.ObterIdentidadeAsync(id, CancellationToken.None);

        identidade.Should().NotBeNull();
        identidade!.Perfil.Should().Be(Perfil.Atendente);
        identidade.GrupoId.Should().NotBeNull();
        contexto.ChangeTracker.Entries().Should().BeEmpty(); // não rastreia: não colide com as gravações da requisição
    }

    // spec perfil-no-cadastro D4: cache de 15 s, apagado na hora por quem grava pelo repositório.
    [Fact]
    public async Task ObterIdentidadeAsync_SegundaLeituraVemDoCache_AteOBancoMudarPorFora()
    {
        var banco = Guid.NewGuid().ToString();
        var id = await SemearUsuarioComGrupoAsync(banco);
        var cache = new MemoryCache(new MemoryCacheOptions());
        await using var contexto = NovoContexto(banco);
        var repositorio = new UsuarioPerfilRepository(contexto, cache);
        (await repositorio.ObterIdentidadeAsync(id, CancellationToken.None))!.Perfil.Should().Be(Perfil.Atendente);

        await using (var porFora = NovoContexto(banco)) // edição direta no banco, sem passar pelo repositório
        {
            var usuario = await porFora.Set<UsuarioPerfil>().SingleAsync(u => u.Id == id);
            usuario.Atualizar("Pessoa", Perfil.Solicitante, null);
            await porFora.SaveChangesAsync();
        }

        (await repositorio.ObterIdentidadeAsync(id, CancellationToken.None))!.Perfil.Should().Be(Perfil.Atendente); // ainda guardado
    }

    [Fact]
    public async Task AtualizarAsync_ApagaDoCache_ALeituraSeguinteVeOCadastroNovo()
    {
        var banco = Guid.NewGuid().ToString();
        var id = await SemearUsuarioComGrupoAsync(banco);
        var cache = new MemoryCache(new MemoryCacheOptions());
        await using var contexto = NovoContexto(banco);
        var repositorio = new UsuarioPerfilRepository(contexto, cache);
        await repositorio.ObterIdentidadeAsync(id, CancellationToken.None); // guarda no cache

        var usuario = (await repositorio.ObterPorIdAsync(id, CancellationToken.None))!;
        usuario.Atualizar("Pessoa", Perfil.Solicitante, null);
        usuario.Desativar();
        await repositorio.AtualizarAsync(usuario, CancellationToken.None);

        var identidade = (await repositorio.ObterIdentidadeAsync(id, CancellationToken.None))!;
        identidade.Perfil.Should().Be(Perfil.Solicitante);
        identidade.Ativo.Should().BeFalse();
        identidade.GrupoId.Should().BeNull();
    }

    [Fact]
    public async Task AdicionarAsync_ApagaDoCache_UsuarioNovoNaoFicaInvisivel()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        await using var contexto = NovoContexto(Guid.NewGuid().ToString());
        var repositorio = new UsuarioPerfilRepository(contexto, cache);
        var novo = new UsuarioPerfil("novo@camarj.com.br", "Novo", Perfil.Atendente);

        (await repositorio.ObterIdentidadeAsync(novo.Id, CancellationToken.None)).Should().BeNull(); // inexistente: não é guardado
        await repositorio.AdicionarAsync(novo, CancellationToken.None);

        (await repositorio.ObterIdentidadeAsync(novo.Id, CancellationToken.None)).Should().NotBeNull();
    }

    [Fact]
    public async Task ObterIdentidadeAsync_UsuarioInexistente_DevolveNulo()
    {
        await using var contexto = NovoContexto(Guid.NewGuid().ToString());
        var repositorio = new UsuarioPerfilRepository(contexto, new MemoryCache(new MemoryCacheOptions()));

        (await repositorio.ObterIdentidadeAsync(Guid.NewGuid(), CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task AtualizarAsync_DuasCopiasDoMesmoUsuarioComGrupoNaMesmaRequisicao_GravaAsDuas()
    {
        var banco = Guid.NewGuid().ToString();
        var id = await SemearUsuarioComGrupoAsync(banco);
        await using var contexto = NovoContexto(banco); // um contexto por requisição, como no DI
        var repositorio = new UsuarioPerfilRepository(contexto, new MemoryCache(new MemoryCacheOptions()));

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
        var repositorio = new UsuarioPerfilRepository(contexto, new MemoryCache(new MemoryCacheOptions()));
        var usuario = (await repositorio.ObterPorIdAsync(id, CancellationToken.None))!;
        usuario.Grupo.Should().NotBeNull();
        usuario.Atualizar("Pessoa", Perfil.Atendente, novaArea);
        await repositorio.AtualizarAsync(usuario, CancellationToken.None);

        await using var conferencia = NovoContexto(banco);
        (await conferencia.Set<UsuarioPerfil>().AsNoTracking().SingleAsync(u => u.Id == id)).GrupoId.Should().Be(novaArea);
        (await conferencia.Set<Grupo>().CountAsync()).Should().Be(trocar ? 2 : 1); // a Área não é recriada nem apagada
    }
}
