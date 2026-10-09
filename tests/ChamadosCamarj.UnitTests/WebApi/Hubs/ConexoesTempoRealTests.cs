using ChamadosCamarj.WebApi.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace ChamadosCamarj.UnitTests.WebApi.Hubs;

/// <summary>spec perfil-no-cadastro D6: o servidor sabe quais conexões abertas são de cada pessoa.</summary>
public class ConexoesTempoRealTests
{
    private static HubCallerContext Contexto(string connectionId, Guid? usuarioId)
    {
        var mock = new Mock<HubCallerContext>();
        mock.SetupGet(c => c.ConnectionId).Returns(connectionId);
        mock.SetupGet(c => c.UserIdentifier).Returns(usuarioId?.ToString());
        return mock.Object;
    }

    [Fact]
    public void DuasConexoesDoMesmoUsuario_RemoveUma_ListaARestante()
    {
        var registro = new ConexoesTempoReal();
        var usuario = Guid.NewGuid();
        registro.Registrar(Contexto("a", usuario), nameof(ChamadosHub));
        registro.Registrar(Contexto("b", usuario), nameof(ChatHub));
        registro.Registrar(Contexto("c", Guid.NewGuid()), nameof(ChamadosHub));

        registro.Remover("a");

        registro.DoUsuario(usuario).Should().ContainSingle().Which.ConnectionId.Should().Be("b");
    }

    [Fact]
    public void ConexaoSemUsuarioValido_NaoEhGuardada()
    {
        var registro = new ConexoesTempoReal();

        registro.Registrar(Contexto("a", null), nameof(ChamadosHub));

        registro.DoUsuario(Guid.Empty).Should().BeEmpty();
    }

    [Fact]
    public async Task RegistroConcorrente_NaoPerdeConexoes()
    {
        var registro = new ConexoesTempoReal();
        var usuario = Guid.NewGuid();

        await Task.WhenAll(Enumerable.Range(0, 200).Select(i =>
            Task.Run(() => registro.Registrar(Contexto($"c{i}", usuario), nameof(ChamadosHub)))));

        registro.DoUsuario(usuario).Should().HaveCount(200);
    }
}
