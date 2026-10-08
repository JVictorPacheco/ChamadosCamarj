using ChamadosCamarj.Application.Features.Relatorios;
using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Relatorios;

/// <summary>De quem são os números do relatório — autorizacao-chamados AC-21 + controle-de-acesso AC-07 (review R-08).</summary>
public class RelatorioEscopoTests
{
    private readonly Mock<IChamadoRepository> _chamadosMock = new();
    private readonly Guid _pedido = Guid.NewGuid();

    private static ContextoAcesso Ctx(Perfil perfil, Guid? id = null) => new(id ?? Guid.NewGuid(), "x@camarj.com.br", perfil, null);

    [Fact]
    public async Task Admin_VeTudo_ERespeitaOFiltroPedido()
    {
        var (resp, ids) = await RelatorioEscopo.ResolverAsync(Ctx(Perfil.Admin), _pedido, _chamadosMock.Object, CancellationToken.None);

        resp.Should().Be(_pedido);
        ids.Should().BeNull();
        _chamadosMock.Verify(c => c.ListarIdsVisiveisAsync(It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Atendente_SoOsProprios_QualquerQueSejaOPedido()
    {
        var atendente = Guid.NewGuid();
        var (resp, ids) = await RelatorioEscopo.ResolverAsync(Ctx(Perfil.Atendente, atendente), _pedido, _chamadosMock.Object, CancellationToken.None);

        resp.Should().Be(atendente);
        ids.Should().BeNull();
    }

    [Fact]
    public async Task Solicitante_SoOsChamadosQueEleVe_EIgnoraOPedido()
    {
        var visiveis = new[] { Guid.NewGuid() };
        var ctx = Ctx(Perfil.Solicitante);
        _chamadosMock.Setup(c => c.ListarIdsVisiveisAsync(ctx, It.IsAny<CancellationToken>())).ReturnsAsync(visiveis);

        var (resp, ids) = await RelatorioEscopo.ResolverAsync(ctx, _pedido, _chamadosMock.Object, CancellationToken.None);

        resp.Should().BeNull();
        ids.Should().BeEquivalentTo(visiveis);
    }
}
