using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Dashboard.Queries;
using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

/// <summary>
/// Métricas do Dashboard pelo módulo, não pelo perfil — spec controle-de-acesso AC-07/AC-12/AC-16
/// (review-3 R-03: o handler de métricas não tinha teste do ModuloGuard). ModuloGuard real; só o cadastro
/// e o repositório são simulados.
/// </summary>
public class ObterMetricasQueryHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IUsuarioPerfilRepository> _usuariosMock = new();
    private readonly ObterMetricasQueryHandler _handler;

    public ObterMetricasQueryHandlerTests()
    {
        _handler = new ObterMetricasQueryHandler(_repositoryMock.Object, _currentUserMock.Object,
            new ModuloGuard(_usuariosMock.Object, _currentUserMock.Object));
    }

    private UsuarioPerfil Logado(Perfil perfil)
    {
        var usuario = new UsuarioPerfil($"{perfil}@camarj.com.br".ToLowerInvariant(), "Pessoa", perfil);
        _currentUserMock.SetupGet(c => c.UsuarioId).Returns(usuario.Id);
        _currentUserMock.SetupGet(c => c.Perfil).Returns(perfil.ToString());
        _currentUserMock.SetupGet(c => c.Email).Returns(usuario.Email);
        _usuariosMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);
        return usuario;
    }

    [Fact]
    public async Task Handle_AtendenteSemModuloDashboard_DeveLancarForbidden_SemConsultarNumeros()
    {
        Logado(Perfil.Atendente).AjustarModulos(ModuloSistema.Nenhum, ModuloSistema.Dashboard);

        var act = () => _handler.Handle(new ObterMetricasQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _repositoryMock.Verify(r => r.ContarResolvidosHojeAsync(It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SolicitanteSemAjuste_DeveLancarForbidden()
    {
        Logado(Perfil.Solicitante); // padrão do Solicitante não tem Dashboard (AC-16)

        var act = () => _handler.Handle(new ObterMetricasQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_SolicitanteComModuloDashboard_ContaSoNoEscopoDeSolicitante()
    {
        Logado(Perfil.Solicitante).AjustarModulos(ModuloSistema.Dashboard, ModuloSistema.Nenhum);
        ContextoAcesso? capturado = null;
        _repositoryMock.Setup(r => r.ContarResolvidosHojeAsync(It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>()))
            .Callback<ContextoAcesso, CancellationToken>((c, _) => capturado = c)
            .ReturnsAsync(2);
        _repositoryMock.Setup(r => r.ContarPorAreaAsync(It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _repositoryMock.Setup(r => r.ContarPorTipoAsync(It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _repositoryMock.Setup(r => r.ContarPorPrioridadeAsync(It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await _handler.Handle(new ObterMetricasQuery(), CancellationToken.None);

        result.TotalResolvidosHoje.Should().Be(2);
        capturado!.Perfil.Should().Be(Perfil.Solicitante);
    }
}
