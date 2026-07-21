using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class AdicionarAnexoHandlerTests
{
    private readonly Mock<IChamadoRepository> _chamadoRepositoryMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();
    private readonly AdicionarAnexoCommandHandler _handler;

    public AdicionarAnexoHandlerTests()
    {
        _handler = new AdicionarAnexoCommandHandler(_chamadoRepositoryMock.Object, _storageServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ComChamadoExistente_DeveFazerUploadERegistrarAnexo()
    {
        var chamadoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        _chamadoRepositoryMock.Setup(r => r.ExisteAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _storageServiceMock.Setup(s => s.UploadAsync(It.IsAny<string>(), "application/pdf", Stream.Null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string caminho, string _, Stream _, CancellationToken _) => caminho);

        var command = new AdicionarAnexoCommand(chamadoId, null, "nota.pdf", "application/pdf", Stream.Null, 2048, usuarioId, "Victor");
        var response = await _handler.Handle(command, CancellationToken.None);

        response.NomeArquivo.Should().Be("nota.pdf");
        response.TamanhoBytes.Should().Be(2048);
        response.EnviadoPorNome.Should().Be("Victor");

        _storageServiceMock.Verify(s => s.UploadAsync(
            It.Is<string>(caminho => caminho.StartsWith(chamadoId.ToString()) && caminho.EndsWith(".pdf")),
            "application/pdf",
            Stream.Null,
            It.IsAny<CancellationToken>()), Times.Once);

        _chamadoRepositoryMock.Verify(r => r.AdicionarAnexoAsync(
            It.Is<Anexo>(a => a.ChamadoId == chamadoId && a.EnviadoPorId == usuarioId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoChamadoNaoExiste_DeveLancarNotFoundExceptionSemFazerUpload()
    {
        var chamadoId = Guid.NewGuid();
        _chamadoRepositoryMock.Setup(r => r.ExisteAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new AdicionarAnexoCommand(chamadoId, null, "nota.pdf", "application/pdf", Stream.Null, 2048);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();

        _storageServiceMock.Verify(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never);
        _chamadoRepositoryMock.Verify(r => r.AdicionarAnexoAsync(It.IsAny<Anexo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComComentarioId_DeveVincularAnexoAoComentario()
    {
        var chamadoId = Guid.NewGuid();
        var comentarioId = Guid.NewGuid();

        _chamadoRepositoryMock.Setup(r => r.ExisteAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _storageServiceMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string caminho, string _, Stream _, CancellationToken _) => caminho);

        var command = new AdicionarAnexoCommand(chamadoId, comentarioId, "foto.jpg", "image/jpeg", Stream.Null, 1024);
        await _handler.Handle(command, CancellationToken.None);

        _chamadoRepositoryMock.Verify(r => r.AdicionarAnexoAsync(
            It.Is<Anexo>(a => a.ComentarioId == comentarioId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
