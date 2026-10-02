using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Interfaces;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

/// <summary>spec area-e-tipo-do-chamado AC-11 — Atendente/Admin corrigem o tipo do chamado.</summary>
public class ReclassificarTipoChamadoHandlerTests
{
    private readonly Mock<IChamadoRepository> _chamadoRepositoryMock = new();
    private readonly Mock<ITipoChamadoRepository> _tipoRepositoryMock = new();
    private readonly Mock<IHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ReclassificarTipoChamadoCommandHandler _handler;
    private readonly Chamado _chamado = new("Título", "Descrição", "Ana", "ana@camarj.com.br", Guid.NewGuid(), Guid.NewGuid());
    private readonly TipoChamado _incidente = new("Incidente", "Erro");

    public ReclassificarTipoChamadoHandlerTests()
    {
        _chamadoRepositoryMock.Setup(r => r.ObterPorIdComTrackingAsync(_chamado.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_chamado);
        _tipoRepositoryMock.Setup(r => r.ObterPorIdAsync(_incidente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_incidente);
        _handler = new ReclassificarTipoChamadoCommandHandler(
            _chamadoRepositoryMock.Object, _tipoRepositoryMock.Object, _historicoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_TrocaOTipo_PersisteERegistraNoHistorico()
    {
        await _handler.Handle(new ReclassificarTipoChamadoCommand(_chamado.Id, _incidente.Id, Guid.NewGuid(), "Atendente"), CancellationToken.None);

        _chamado.TipoId.Should().Be(_incidente.Id);
        _chamadoRepositoryMock.Verify(r => r.AtualizarAsync(_chamado, It.IsAny<CancellationToken>()), Times.Once);
        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<HistoricoEntrada>(h => h.Acao == AcaoHistorico.TipoReclassificado && h.DetalheNovo == "Incidente"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TipoInativo_NaoAceita()
    {
        // "Não classificado" é inativo: serve só para chamados antigos, não para reclassificar.
        _incidente.Desativar();

        var act = () => _handler.Handle(new ReclassificarTipoChamadoCommand(_chamado.Id, _incidente.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _chamadoRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Chamado>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TipoInexistente_NaoAceita()
    {
        var act = () => _handler.Handle(new ReclassificarTipoChamadoCommand(_chamado.Id, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
