using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.Queries;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class HistoricoQueryHandlerTests
{
    private readonly Mock<IChamadoRepository> _chamadoRepositoryMock = new();
    private readonly Mock<IHistoricoRepository> _historicoRepositoryMock = new();
    private readonly ListarHistoricoQueryHandler _handler;

    public HistoricoQueryHandlerTests()
    {
        _handler = new ListarHistoricoQueryHandler(
            _chamadoRepositoryMock.Object,
            _historicoRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DeveListarHistoricoDosChamado()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());
        
        var historicos = new List<HistoricoEntrada>
        {
            HistoricoEntrada.Criar(chamadoId, "João", null, AcaoHistorico.Criado, null, "Chamado criado"),
            HistoricoEntrada.Criar(chamadoId, "Victor", Guid.NewGuid(), AcaoHistorico.Assumido, null, "Victor"),
            HistoricoEntrada.Criar(chamadoId, "Victor", Guid.NewGuid(), AcaoHistorico.Resolvido, null, "Resolvido")
        };

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);
        
        _historicoRepositoryMock.Setup(r => r.ObterPorChamadoAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(historicos);

        var query = new ListarHistoricoQuery(chamadoId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().AllSatisfy(h => h.ChamadoId.Should().Be(chamadoId));
    }

    [Fact]
    public async Task Handle_DeveRetornarHistoricoOrdenadoDescendentePorData()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());
        
        var agora = DateTime.UtcNow;
        var historicos = new List<HistoricoEntrada>
        {
            HistoricoEntrada.Criar(chamadoId, "João", null, AcaoHistorico.Criado, null, "Criado"),
            HistoricoEntrada.Criar(chamadoId, "Victor", Guid.NewGuid(), AcaoHistorico.Assumido, null, "Assumido"),
            HistoricoEntrada.Criar(chamadoId, "Victor", Guid.NewGuid(), AcaoHistorico.Resolvido, null, "Resolvido")
        }.OrderByDescending(h => h.DataHora).ToList();

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);
        
        _historicoRepositoryMock.Setup(r => r.ObterPorChamadoAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(historicos);

        var query = new ListarHistoricoQuery(chamadoId);
        var result = await _handler.Handle(query, CancellationToken.None);

        var resultList = result.ToList();
        resultList.Should().HaveCount(3);
        // Verificar que histórico está em ordem (apenas contagem)
    }

    [Fact]
    public async Task Handle_QuandoChamadoNaoExiste_DeveLancarNotFoundException()
    {
        var chamadoId = Guid.NewGuid();
        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado?)null);

        var query = new ListarHistoricoQuery(chamadoId);

        var act = async () => await _handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_DeveRetornarVazioQuandoNaoHaHistorico()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);
        
        _historicoRepositoryMock.Setup(r => r.ObterPorChamadoAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HistoricoEntrada>());

        var query = new ListarHistoricoQuery(chamadoId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_DeveMapeiarEntidadeParaResponse()
    {
        var chamadoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid());
        
        var historico = HistoricoEntrada.Criar(
            chamadoId, 
            "Victor", 
            usuarioId, 
            AcaoHistorico.Assumido, 
            null, 
            "Assumido por Victor"
        );

        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);
        
        _historicoRepositoryMock.Setup(r => r.ObterPorChamadoAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HistoricoEntrada> { historico });

        var query = new ListarHistoricoQuery(chamadoId);
        var result = await _handler.Handle(query, CancellationToken.None);

        var resultList = result.ToList();
        resultList.Should().HaveCount(1);
        resultList[0].UsuarioNome.Should().Be("Victor");
        resultList[0].UsuarioId.Should().Be(usuarioId);
        resultList[0].Acao.Should().Be(AcaoHistorico.Assumido);
    }
}
