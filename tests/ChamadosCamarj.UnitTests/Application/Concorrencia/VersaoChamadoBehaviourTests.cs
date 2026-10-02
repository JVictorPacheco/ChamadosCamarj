using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Behaviours;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Interfaces;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Application.Features.Chamados.Queries;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.WebApi.Services;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Concorrencia;

/// <summary>Edição simultânea — spec correcoes-pre-deploy AC-09, AC-10, AC-13, AC-23.</summary>
public class VersaoChamadoBehaviourTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly Mock<IVersaoLidaAccessor> _versaoLidaMock = new();
    private readonly Guid _chamadoId = Guid.NewGuid();
    private bool _handlerChamado;

    private void VersaoAtual(string versao) =>
        _repositoryMock.Setup(r => r.ObterVersaoAsync(_chamadoId, It.IsAny<CancellationToken>())).ReturnsAsync(versao);

    private void VersaoLida(string? versao) => _versaoLidaMock.SetupGet(v => v.VersaoLida).Returns(versao);

    private Task<Unit> Executar<TRequest>(TRequest request) where TRequest : notnull
    {
        var behaviour = new VersaoChamadoBehaviour<TRequest, Unit>(_repositoryMock.Object, _versaoLidaMock.Object);
        return behaviour.Handle(request, _ => { _handlerChamado = true; return Task.FromResult(Unit.Value); }, CancellationToken.None);
    }

    [Fact]
    public async Task MesmaVersao_SegueParaOHandler()
    {
        VersaoAtual("100");
        VersaoLida("100");

        await Executar(new ResolverChamadoCommand(_chamadoId));

        _handlerChamado.Should().BeTrue();
    }

    [Fact]
    public async Task VersaoDiferente_Recusa409ComAMensagemDoAC10()
    {
        // AC-09: B alterou depois que A leu.
        VersaoAtual("200");
        VersaoLida("100");

        var act = () => Executar(new AlterarStatusChamadoCommand(_chamadoId, StatusChamado.EmAndamento));

        await act.Should().ThrowAsync<ConflictException>().WithMessage(VersaoChamado.MensagemConflito);
        _handlerChamado.Should().BeFalse();
    }

    [Fact]
    public async Task SemVersaoInformada_SegueSemConsultar()
    {
        // AC-23: cliente antigo, sem If-Match.
        VersaoLida(null);

        await Executar(new ResolverChamadoCommand(_chamadoId));

        _handlerChamado.Should().BeTrue();
        _repositoryMock.Verify(r => r.ObterVersaoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Comentario_NuncaDaConflito()
    {
        // AC-13
        VersaoAtual("200");
        VersaoLida("100");

        await Executar(new ComentarChamadoCommand(_chamadoId, "Ana", "texto"));

        _handlerChamado.Should().BeTrue();
    }

    [Fact]
    public async Task RequestQueNaoEhDeChamado_Segue()
    {
        VersaoLida("100");

        await Executar(new ListarChamadosQuery());

        _handlerChamado.Should().BeTrue();
        _repositoryMock.Verify(r => r.ObterVersaoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("\"123\"", "123")]
    [InlineData("123", "123")]
    [InlineData(" \"123\" ", "123")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Accessor_NormalizaOIfMatch(string? cabecalho, string? esperado)
    {
        VersaoLidaAccessor.Normalizar(cabecalho).Should().Be(esperado);
    }
}
