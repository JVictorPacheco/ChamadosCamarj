using System.Text.Json;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Interfaces;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

/// <summary>
/// spec correcoes-acesso-chamados AC-01 (edição com tracking, sem 409 falso) e spec editar-chamado
/// AC-16..AC-18 (sem mudança não grava; histórico com antes e depois só do que mudou).
/// </summary>
public class AtualizarChamadoHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly Mock<IHistoricoRepository> _historicoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly List<HistoricoEntrada> _historico = [];
    private readonly AtualizarChamadoCommandHandler _handler;
    private readonly Guid _id = Guid.NewGuid();
    private readonly Chamado _chamado = new("Antigo", "Descrição antiga", "Ana", "ana@camarj.com.br", Guid.NewGuid(), Guid.NewGuid());

    public AtualizarChamadoHandlerTests()
    {
        _handler = new AtualizarChamadoCommandHandler(_repositoryMock.Object, _historicoMock.Object, _unitOfWorkMock.Object);
        _repositoryMock.Setup(r => r.ObterPorIdComTrackingAsync(_id, It.IsAny<CancellationToken>())).ReturnsAsync(_chamado);
        _historicoMock.Setup(h => h.AdicionarAsync(It.IsAny<HistoricoEntrada>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HistoricoEntrada h, CancellationToken _) => { _historico.Add(h); return h; });
    }

    private static Dictionary<string, string> Ler(string? json) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(json!)!;

    [Fact]
    public async Task Handle_DeveBuscarComTracking_EPersistirOsDadosNovos()
    {
        await _handler.Handle(new AtualizarChamadoCommand(_id, "Novo", "Descrição nova"), CancellationToken.None);

        _repositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.AtualizarAsync(
            It.Is<Chamado>(c => c.Titulo == "Novo" && c.Descricao == "Descrição nova"), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoChamadoNaoExiste_DeveLancarNotFound()
    {
        var act = () => _handler.Handle(new AtualizarChamadoCommand(Guid.NewGuid(), "T", "D"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_SoTitulo_RegistraHistoricoSoComOTitulo()
    {
        // AC-17 / AC-18
        var usuarioId = Guid.NewGuid();
        await _handler.Handle(new AtualizarChamadoCommand(_id, "Novo título", "Descrição antiga", usuarioId, "Bruno"), CancellationToken.None);

        var entrada = _historico.Should().ContainSingle().Subject;
        entrada.Acao.Should().Be(AcaoHistorico.ChamadoEditado);
        entrada.UsuarioId.Should().Be(usuarioId);
        entrada.UsuarioNome.Should().Be("Bruno");
        Ler(entrada.DetalheAnterior).Should().Equal(new Dictionary<string, string> { ["titulo"] = "Antigo" });
        Ler(entrada.DetalheNovo).Should().Equal(new Dictionary<string, string> { ["titulo"] = "Novo título" });
    }

    [Fact]
    public async Task Handle_TituloEDescricao_RegistraOsDois()
    {
        await _handler.Handle(new AtualizarChamadoCommand(_id, "Novo", "Descrição nova"), CancellationToken.None);

        var entrada = _historico.Should().ContainSingle().Subject;
        Ler(entrada.DetalheAnterior).Should().Equal(new Dictionary<string, string> { ["titulo"] = "Antigo", ["descricao"] = "Descrição antiga" });
        Ler(entrada.DetalheNovo).Should().Equal(new Dictionary<string, string> { ["titulo"] = "Novo", ["descricao"] = "Descrição nova" });
    }

    [Fact]
    public async Task Handle_SemMudanca_NaoGravaNemRegistra()
    {
        // AC-16
        await _handler.Handle(new AtualizarChamadoCommand(_id, "Antigo", "Descrição antiga"), CancellationToken.None);

        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Chamado>(), It.IsAny<CancellationToken>()), Times.Never);
        _historico.Should().BeEmpty();
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
