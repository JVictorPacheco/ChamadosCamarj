using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Features.Chamados.Queries;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ObterChamadoPorIdHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly ObterChamadoPorIdQueryHandler _handler;

    public ObterChamadoPorIdHandlerTests()
    {
        _currentUserMock.SetupGet(c => c.Perfil).Returns("Atendente");
        _handler = new ObterChamadoPorIdQueryHandler(_repositoryMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_QuandoChamadoExiste_DeveRetornarResponse()
    {
        var chamadoId = Guid.NewGuid();
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid(), Guid.NewGuid());

        _repositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var result = await _handler.Handle(new ObterChamadoPorIdQuery(chamadoId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Titulo.Should().Be("Título");
    }

    [Fact]
    public async Task Handle_QuandoChamadoNaoExiste_DeveRetornarNull()
    {
        _repositoryMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Chamado?)null);

        var result = await _handler.Handle(new ObterChamadoPorIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    private Chamado ChamadoComComentarios(Guid id)
    {
        var chamado = new Chamado("Título", "Descrição", "João", "joao@camarj.com.br", Guid.NewGuid(), Guid.NewGuid());
        chamado.Comentarios.Add(new Comentario(id, "Ana", "público 1"));
        chamado.Comentarios.Add(new Comentario(id, "Ana", "público 2"));
        chamado.Comentarios.Add(new Comentario(id, "Atendente", "interno", TipoComentario.Interno));
        _repositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(chamado);
        return chamado;
    }

    [Fact]
    public async Task Handle_ParaSolicitante_QuantidadeDeComentariosNaoContaOsInternos()
    {
        // spec correcoes-acesso-chamados AC-02
        _currentUserMock.SetupGet(c => c.Perfil).Returns("Solicitante");
        var id = Guid.NewGuid();
        ChamadoComComentarios(id);

        var result = await _handler.Handle(new ObterChamadoPorIdQuery(id), CancellationToken.None);

        result!.QuantidadeComentarios.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ParaAtendente_QuantidadeDeComentariosContaTodos()
    {
        var id = Guid.NewGuid();
        ChamadoComComentarios(id);

        var result = await _handler.Handle(new ObterChamadoPorIdQuery(id), CancellationToken.None);

        result!.QuantidadeComentarios.Should().Be(3);
    }
}
