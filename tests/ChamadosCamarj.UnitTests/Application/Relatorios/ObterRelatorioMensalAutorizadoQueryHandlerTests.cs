using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Relatorios.DTOs;
using ChamadosCamarj.Application.Features.Relatorios.Queries;
using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Relatorios;

/// <summary>
/// Ligação módulo + escopo + relatório (review-2 R-06): com o ModuloGuard REAL (só o cadastro simulado),
/// confere o que chega ao relatório para cada perfil.
/// </summary>
public class ObterRelatorioMensalAutorizadoQueryHandlerTests
{
    private readonly Mock<IUsuarioPerfilRepository> _usuariosMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IChamadoRepository> _chamadosMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private ObterRelatorioMensalQuery? _enviada;

    public ObterRelatorioMensalAutorizadoQueryHandlerTests()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ObterRelatorioMensalQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<RelatorioMensalResponse>, CancellationToken>((q, _) => _enviada = (ObterRelatorioMensalQuery)q)
            .ReturnsAsync((RelatorioMensalResponse)null!);
    }

    private ObterRelatorioMensalAutorizadoQueryHandler Handler() =>
        new(new ModuloGuard(_usuariosMock.Object, _currentUserMock.Object), _currentUserMock.Object, _chamadosMock.Object, _mediatorMock.Object);

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
    public async Task Solicitante_SemModulo_Forbidden_ENaoMontaORelatorio()
    {
        Logado(Perfil.Solicitante);

        var act = () => Handler().Handle(new ObterRelatorioMensalAutorizadoQuery(2026, 10), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _enviada.Should().BeNull();
    }

    [Fact]
    public async Task Solicitante_ComModulo_RecebeSoOsChamadosQueVe()
    {
        Logado(Perfil.Solicitante).AjustarModulos(ModuloSistema.RelatorioMensal, ModuloSistema.Nenhum);
        var visiveis = new[] { Guid.NewGuid() };
        _chamadosMock.Setup(c => c.ListarIdsVisiveisAsync(It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>())).ReturnsAsync(visiveis);

        await Handler().Handle(new ObterRelatorioMensalAutorizadoQuery(2026, 10, Guid.NewGuid()), CancellationToken.None);

        _enviada!.IdsVisiveis.Should().BeEquivalentTo(visiveis);
        _enviada.ResponsavelId.Should().BeNull();
    }

    [Fact]
    public async Task Atendente_ForcadoAosProprios()
    {
        var atendente = Logado(Perfil.Atendente);

        await Handler().Handle(new ObterRelatorioMensalAutorizadoQuery(2026, 10, Guid.NewGuid()), CancellationToken.None);

        _enviada!.ResponsavelId.Should().Be(atendente.Id);
        _enviada.IdsVisiveis.Should().BeNull();
    }

    [Fact]
    public async Task Atendente_SemModulo_Forbidden()
    {
        Logado(Perfil.Atendente).AjustarModulos(ModuloSistema.Nenhum, ModuloSistema.RelatorioMensal);

        var act = () => Handler().Handle(new ObterRelatorioMensalAutorizadoQuery(2026, 10), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
