using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Features.Chamados.Queries;
using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ListarChamadosQueryHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly ListarChamadosQueryHandler _handler;

    private static readonly Guid UsuarioId = Guid.NewGuid();
    private static readonly Guid GrupoId = Guid.NewGuid();

    public ListarChamadosQueryHandlerTests()
    {
        _currentUserMock.SetupGet(c => c.UsuarioId).Returns(UsuarioId);
        _currentUserMock.SetupGet(c => c.Email).Returns("ana.colaboradora@camarj.com.br");
        _currentUserMock.SetupGet(c => c.Perfil).Returns("Solicitante");
        _currentUserMock.SetupGet(c => c.GrupoId).Returns(GrupoId);

        _handler = new ListarChamadosQueryHandler(_repositoryMock.Object, _currentUserMock.Object);
    }

    private void SetupListar(
        Action<ContextoAcesso, IEnumerable<StatusChamado>?, DateTime?, DateTime?>? capturar = null,
        IEnumerable<Chamado>? itens = null)
    {
        var lista = itens?.ToList() ?? [];
        _repositoryMock
            .Setup(r => r.ListarAsync(
                It.IsAny<ContextoAcesso>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StatusChamado?>(),
                It.IsAny<PrioridadeChamado?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<IEnumerable<StatusChamado>?>(), It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(), It.IsAny<MotivoEncerramento?>(), It.IsAny<CancellationToken>()))
            .Callback<ContextoAcesso, int, int, StatusChamado?, PrioridadeChamado?, Guid?, Guid?, Guid?, string?, string?, IEnumerable<StatusChamado>?, DateTime?, DateTime?, MotivoEncerramento?, CancellationToken>(
                (acesso, _, _, _, _, _, _, _, _, _, statusEntre, dataInicio, dataFim, _, _) =>
                    capturar?.Invoke(acesso, statusEntre, dataInicio, dataFim))
            .ReturnsAsync((lista, lista.Count));
    }

    [Fact]
    public async Task Handle_DevePassarOContextoDoUsuarioLogadoParaORepositorio()
    {
        // AC-01/AC-02: a regra de visibilidade usa SEMPRE o usuário do token, nunca parâmetro da query.
        ContextoAcesso? capturado = null;
        SetupListar((acesso, _, _, _) => capturado = acesso);

        await _handler.Handle(new ListarChamadosQuery(), CancellationToken.None);

        capturado.Should().Be(new ContextoAcesso(UsuarioId, "ana.colaboradora@camarj.com.br", Perfil.Solicitante, GrupoId));
    }

    [Fact]
    public async Task Handle_ComPerfilDesconhecidoNoToken_DeveTratarComoSolicitante()
    {
        _currentUserMock.SetupGet(c => c.Perfil).Returns("Qualquer");
        ContextoAcesso? capturado = null;
        SetupListar((acesso, _, _, _) => capturado = acesso);

        await _handler.Handle(new ListarChamadosQuery(), CancellationToken.None);

        capturado!.Perfil.Should().Be(Perfil.Solicitante);
    }

    [Fact]
    public async Task Handle_DevePassarSolicitanteEmailComoFiltroParaORepositorio()
    {
        SetupListar();

        var query = new ListarChamadosQuery(SolicitanteEmail: "ana.colaboradora@camarj.com.br");
        await _handler.Handle(query, CancellationToken.None);

        _repositoryMock.Verify(r => r.ListarAsync(
            It.IsAny<ContextoAcesso>(), 1, 10, null, null, null, null, null, null, "ana.colaboradora@camarj.com.br",
            null, null, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DeveMapearOsChamadosRetornadosPeloRepositorio()
    {
        var chamado = new Chamado("Título", "Descrição", "Ana", "ana.colaboradora@camarj.com.br", Guid.NewGuid(), Guid.NewGuid());
        SetupListar(itens: [chamado]);

        var result = await _handler.Handle(new ListarChamadosQuery(), CancellationToken.None);

        result.Total.Should().Be(1);
        result.Items.Should().ContainSingle(c => c.SolicitanteEmail == "ana.colaboradora@camarj.com.br");
    }

    [Theory]
    [InlineData("Solicitante", 1)]
    [InlineData("Atendente", 2)]
    public async Task Handle_QuantidadeDeComentarios_SoContaInternosParaQuemPodeVelos(string perfil, int esperado)
    {
        // spec correcoes-acesso-chamados AC-02 (review R-04: a listagem também precisa de teste)
        _currentUserMock.SetupGet(c => c.Perfil).Returns(perfil);
        var chamado = new Chamado("Título", "Descrição", "Ana", "ana.colaboradora@camarj.com.br", Guid.NewGuid(), Guid.NewGuid());
        chamado.Comentarios.Add(new Comentario(chamado.Id, "Ana", "público"));
        chamado.Comentarios.Add(new Comentario(chamado.Id, "Atendente", "interno", TipoComentario.Interno));
        SetupListar(itens: [chamado]);

        var result = await _handler.Handle(new ListarChamadosQuery(), CancellationToken.None);

        result.Items.Single().QuantidadeComentarios.Should().Be(esperado);
    }

    [Fact]
    public async Task Handle_ComFinalizadosTrue_DevePassarOsTresStatusFinalizadosParaORepositorio()
    {
        IEnumerable<StatusChamado>? statusCapturado = null;
        SetupListar((_, statusEntre, _, _) => statusCapturado = statusEntre);

        await _handler.Handle(new ListarChamadosQuery(Finalizados: true), CancellationToken.None);

        statusCapturado.Should().BeEquivalentTo([StatusChamado.Resolvido, StatusChamado.Fechado, StatusChamado.Cancelado]);
    }

    [Fact]
    public async Task Handle_SemFinalizados_NaoDevePassarFiltroDeStatusEntre()
    {
        IEnumerable<StatusChamado>? statusCapturado = [StatusChamado.Aberto];
        SetupListar((_, statusEntre, _, _) => statusCapturado = statusEntre);

        await _handler.Handle(new ListarChamadosQuery(), CancellationToken.None);

        statusCapturado.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DevePassarDataInicioEDataFimComoUtcParaORepositorio()
    {
        var inicio = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var fim = new DateTime(2026, 7, 31, 0, 0, 0, DateTimeKind.Unspecified);

        DateTime? inicioCapturado = null;
        DateTime? fimCapturado = null;
        SetupListar((_, _, dataInicio, dataFim) =>
        {
            inicioCapturado = dataInicio;
            fimCapturado = dataFim;
        });

        await _handler.Handle(new ListarChamadosQuery(DataInicio: inicio, DataFim: fim), CancellationToken.None);

        inicioCapturado!.Value.Kind.Should().Be(DateTimeKind.Utc);
        inicioCapturado!.Value.Should().Be(new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc));

        fimCapturado!.Value.Kind.Should().Be(DateTimeKind.Utc);
        fimCapturado!.Value.Date.Should().Be(new DateTime(2026, 7, 31, 0, 0, 0, DateTimeKind.Utc));
        fimCapturado!.Value.TimeOfDay.Should().BeGreaterThan(new TimeSpan(0, 23, 59, 59));
    }
}
