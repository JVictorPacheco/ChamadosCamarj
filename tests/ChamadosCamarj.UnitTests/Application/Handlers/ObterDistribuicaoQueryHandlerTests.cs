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
    private readonly ObterDistribuicaoQueryHandler _handler;

    public ObterDistribuicaoQueryHandlerTests()
    {
        _currentUserMock.SetupGet(c => c.Perfil).Returns("Atendente");
        _currentUserMock.SetupGet(c => c.Email).Returns("atendente@camarj.com.br");
        _handler = new ObterDistribuicaoQueryHandler(_repositoryMock.Object, _currentUserMock.Object);
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
        _currentUserMock.SetupGet(c => c.Perfil).Returns("Solicitante");

        var act = async () => await _handler.Handle(new ObterDistribuicaoQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _repositoryMock.Verify(r => r.ContarPorStatusAgrupadoAsync(It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
