using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;
using ChamadosCamarj.Application.Common.Interfaces;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class AbrirChamadoHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly Mock<IGrupoRepository> _grupoRepositoryMock = new();
    private readonly Mock<ITipoChamadoRepository> _tipoRepositoryMock = new();
    private readonly Mock<IHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly AbrirChamadoCommandHandler _handler;

    private readonly Grupo _area = new("Reembolso", "Área de reembolso");
    private readonly TipoChamado _tipo = new("Incidente", "Algo com erro");

    public AbrirChamadoHandlerTests()
    {
        _repositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Chamado>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado c, CancellationToken _) => c);

        _grupoRepositoryMock.Setup(r => r.ObterPorIdAsync(_area.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_area);
        _tipoRepositoryMock.Setup(r => r.ObterPorIdAsync(_tipo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_tipo);

        _handler = new AbrirChamadoCommandHandler(
            _repositoryMock.Object,
            _grupoRepositoryMock.Object,
            _tipoRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _publisherMock.Object,
            _unitOfWorkMock.Object);
    }

    private AbrirChamadoCommand Comando(PrioridadeChamado prioridade = PrioridadeChamado.Media, Guid? areaId = null, Guid? tipoId = null) =>
        new("Problema de acesso", "Não consigo acessar o sistema.", "João", "joao@camarj.com.br",
            areaId ?? _area.Id, tipoId ?? _tipo.Id, prioridade);

    [Fact]
    public async Task Handle_DeveCriarChamadoERetornarResponse()
    {
        var result = await _handler.Handle(Comando(PrioridadeChamado.Alta), CancellationToken.None);

        result.Should().NotBeNull();
        result.Titulo.Should().Be("Problema de acesso");
        result.SolicitanteEmail.Should().Be("joao@camarj.com.br");
        result.Status.Should().Be(StatusChamado.Aberto);
        result.AreaId.Should().Be(_area.Id);
        result.TipoId.Should().Be(_tipo.Id);
        result.AreaNome.Should().Be("Reembolso");
        result.TipoNome.Should().Be("Incidente");
    }

    [Fact]
    public async Task Handle_DeveCallAdicionarAsyncUmaVez()
    {
        await _handler.Handle(Comando(), CancellationToken.None);

        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Chamado>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComPrioridadeAlta_DeveCalcularDataLimiteEm24h()
    {
        var antes = DateTime.UtcNow;
        var result = await _handler.Handle(Comando(PrioridadeChamado.Alta), CancellationToken.None);

        result.DataLimite.Should().NotBeNull();
        result.DataLimite!.Value.Should().BeOnOrAfter(antes.AddHours(24));
        result.DataLimite!.Value.Should().BeOnOrBefore(DateTime.UtcNow.AddHours(24).AddSeconds(1));
    }

    [Fact]
    public async Task Handle_QuandoAreaNaoExiste_DeveLancarNotFoundException()
    {
        var act = async () => await _handler.Handle(Comando(areaId: Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Chamado>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoAreaInativa_DeveLancarNotFoundException()
    {
        _area.Desativar();

        var act = async () => await _handler.Handle(Comando(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoTipoInativo_DeveLancarNotFoundException()
    {
        // "Não classificado" é inativo: existe só para chamados antigos, não para aberturas novas.
        _tipo.Desativar();

        var act = async () => await _handler.Handle(Comando(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Chamado>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
