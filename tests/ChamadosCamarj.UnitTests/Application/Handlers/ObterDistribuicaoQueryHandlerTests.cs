using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Dashboard.Queries;
using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ObterDistribuicaoQueryHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IUsuarioPerfilRepository> _usuariosMock = new();
    private readonly ObterDistribuicaoQueryHandler _handler;

    public ObterDistribuicaoQueryHandlerTests()
    {
        Logado(Perfil.Atendente, "atendente@camarj.com.br");
        // ModuloGuard real (spec controle-de-acesso): só o cadastro é simulado.
        _handler = new ObterDistribuicaoQueryHandler(_repositoryMock.Object, _currentUserMock.Object,
            new ModuloGuard(_usuariosMock.Object, _currentUserMock.Object));
    }

    private UsuarioPerfil Logado(Perfil perfil, string email)
    {
        var usuario = new UsuarioPerfil(email, "Pessoa", perfil);
        _currentUserMock.SetupGet(c => c.UsuarioId).Returns(usuario.Id);
        _currentUserMock.SetupGet(c => c.Perfil).Returns(perfil.ToString());
        _currentUserMock.SetupGet(c => c.Email).Returns(email);
        _usuariosMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);
        return usuario;
    }

    [Fact]
    public async Task Handle_DeveMapearContagensPorStatusEmUmaUnicaChamadaAoRepositorio()
    {
        _repositoryMock.Setup(r => r.ContarPorStatusAgrupadoAsync(It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<StatusChamado, int>
            {
                [StatusChamado.Aberto] = 3,
                [StatusChamado.EmAndamento] = 5,
                [StatusChamado.Resolvido] = 2,
                [StatusChamado.Fechado] = 7,
                [StatusChamado.Cancelado] = 1
            });

        var result = await _handler.Handle(new ObterDistribuicaoQuery(), CancellationToken.None);

        result.Aguardando.Should().Be(3);
        result.Assumido.Should().Be(5);
        result.Resolvido.Should().Be(2);
        result.Encerrado.Should().Be(7);
        result.Cancelado.Should().Be(1);

        _repositoryMock.Verify(r => r.ContarPorStatusAgrupadoAsync(It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.ContarPorStatusAsync(It.IsAny<StatusChamado>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComStatusSemNenhumChamado_DeveRetornarZeroParaEsseStatus()
    {
        // Nem todo status precisa aparecer no dicionário agrupado (GroupBy só retorna chaves existentes)
        _repositoryMock.Setup(r => r.ContarPorStatusAgrupadoAsync(It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<StatusChamado, int>
            {
                [StatusChamado.Aberto] = 4
            });

        var result = await _handler.Handle(new ObterDistribuicaoQuery(), CancellationToken.None);

        result.Aguardando.Should().Be(4);
        result.Assumido.Should().Be(0);
        result.Resolvido.Should().Be(0);
        result.Encerrado.Should().Be(0);
        result.Cancelado.Should().Be(0);
    }

    [Fact]
    public async Task Handle_DeveContarNoEscopoDoUsuarioLogado()
    {
        // AC-13: os números do Dashboard contam só o que o usuário pode ver.
        ContextoAcesso? capturado = null;
        _repositoryMock.Setup(r => r.ContarPorStatusAgrupadoAsync(It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>()))
            .Callback<ContextoAcesso, CancellationToken>((acesso, _) => capturado = acesso)
            .ReturnsAsync(new Dictionary<StatusChamado, int>());

        await _handler.Handle(new ObterDistribuicaoQuery(), CancellationToken.None);

        capturado!.Perfil.Should().Be(Perfil.Atendente);
        capturado.Email.Should().Be("atendente@camarj.com.br");
    }

    [Fact]
    public async Task Handle_QuandoSolicitante_DeveLancarForbidden()
    {
        // Sem ajuste, Solicitante continua sem Dashboard (controle-de-acesso AC-16).
        Logado(Perfil.Solicitante, "sol@camarj.com.br");

        var act = async () => await _handler.Handle(new ObterDistribuicaoQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _repositoryMock.Verify(r => r.ContarPorStatusAgrupadoAsync(It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // spec controle-de-acesso AC-07: Solicitante que ganhou o Dashboard vê os números — contados no
    // escopo dele (o ContextoAcesso dele vai para o repositório).
    [Fact]
    public async Task Handle_SolicitanteComModuloDashboard_VeOsNumerosDoProprioEscopo()
    {
        Logado(Perfil.Solicitante, "sol@camarj.com.br").AjustarModulos(ModuloSistema.Dashboard, ModuloSistema.Nenhum);
        ContextoAcesso? capturado = null;
        _repositoryMock.Setup(r => r.ContarPorStatusAgrupadoAsync(It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>()))
            .Callback<ContextoAcesso, CancellationToken>((acesso, _) => capturado = acesso)
            .ReturnsAsync(new Dictionary<StatusChamado, int>());

        await _handler.Handle(new ObterDistribuicaoQuery(), CancellationToken.None);

        capturado!.Perfil.Should().Be(Perfil.Solicitante);
        capturado.Email.Should().Be("sol@camarj.com.br");
    }

    // AC-12: Atendente a quem o Admin tirou o Dashboard é recusado, mesmo com o token de Atendente.
    [Fact]
    public async Task Handle_AtendenteSemModuloDashboard_DeveLancarForbidden()
    {
        Logado(Perfil.Atendente, "ate@camarj.com.br").AjustarModulos(ModuloSistema.Nenhum, ModuloSistema.Dashboard);

        var act = () => _handler.Handle(new ObterDistribuicaoQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
