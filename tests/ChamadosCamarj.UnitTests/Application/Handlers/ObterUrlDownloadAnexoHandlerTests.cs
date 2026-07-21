using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.Queries;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ObterUrlDownloadAnexoHandlerTests
{
    private readonly Mock<IChamadoRepository> _chamadoRepositoryMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();
    private readonly ObterUrlDownloadAnexoQueryHandler _handler;

    public ObterUrlDownloadAnexoHandlerTests()
    {
        _handler = new ObterUrlDownloadAnexoQueryHandler(_chamadoRepositoryMock.Object, _storageServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ComAnexoExistente_DeveGerarUrlAssinadaComCaminhoCorreto()
    {
        var anexoId = Guid.NewGuid();
        var chamadoId = Guid.NewGuid();
        var anexo = new Anexo(chamadoId, "nota.pdf", $"{chamadoId}/abc.pdf", "application/pdf", 1024);

        _chamadoRepositoryMock.Setup(r => r.ObterAnexoPorIdAsync(anexoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(anexo);
        _storageServiceMock.Setup(s => s.ObterUrlAssinadaAsync($"{chamadoId}/abc.pdf", 3600, It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.example/signed-url");

        var url = await _handler.Handle(new ObterUrlDownloadAnexoQuery(anexoId), CancellationToken.None);

        url.Should().Be("https://storage.example/signed-url");
    }

    [Fact]
    public async Task Handle_QuandoAnexoNaoExiste_DeveLancarNotFoundException()
    {
        var anexoId = Guid.NewGuid();
        _chamadoRepositoryMock.Setup(r => r.ObterAnexoPorIdAsync(anexoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Anexo?)null);

        var act = async () => await _handler.Handle(new ObterUrlDownloadAnexoQuery(anexoId), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
