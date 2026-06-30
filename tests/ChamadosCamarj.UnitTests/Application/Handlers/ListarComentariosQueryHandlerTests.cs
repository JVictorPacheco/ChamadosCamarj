using ChamadosCamarj.Application.Features.Chamados.Queries;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ListarComentariosQueryHandlerTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly ListarComentariosQueryHandler _handler;

    public ListarComentariosQueryHandlerTests()
    {
        _handler = new ListarComentariosQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DeveMapearComentariosParaResponse()
    {
        var chamadoId = Guid.NewGuid();
        var comentario1 = new Comentario(chamadoId, "Victor", "Primeiro comentário.", TipoComentario.Publico);
        var comentario2 = new Comentario(chamadoId, "Fábio", "Nota interna.", TipoComentario.Interno);

        _repositoryMock.Setup(r => r.ObterComentariosPorChamadoAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { comentario1, comentario2 });

        var resultado = await _handler.Handle(new ListarComentariosQuery(chamadoId), CancellationToken.None);

        resultado.Should().HaveCount(2);
        resultado.Should().ContainSingle(r => r.Autor == "Victor" && r.Tipo == TipoComentario.Publico);
        resultado.Should().ContainSingle(r => r.Autor == "Fábio" && r.Tipo == TipoComentario.Interno);
    }

    [Fact]
    public async Task Handle_QuandoNaoHaComentarios_DeveRetornarVazio()
    {
        var chamadoId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.ObterComentariosPorChamadoAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Comentario>());

        var resultado = await _handler.Handle(new ListarComentariosQuery(chamadoId), CancellationToken.None);

        resultado.Should().BeEmpty();
    }
}
