using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.Commands.EditarMensagem;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class EditarMensagemHandlerTests
{
    private readonly Mock<IChatMensagemRepository> _mensagemRepositoryMock = new();
    private readonly Mock<IChatHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly EditarMensagemCommandHandler _handler;

    public EditarMensagemHandlerTests()
    {
        _handler = new EditarMensagemCommandHandler(
            _mensagemRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_QuandoMensagemNaoExiste_DeveLancarNotFoundException()
    {
        var mensagemId = Guid.NewGuid();
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatMensagem?)null);

        var command = new EditarMensagemCommand(mensagemId, "novo texto");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoNaoEhOAutor_DeveLancarForbiddenException()
    {
        var autorId = Guid.NewGuid();
        var mensagem = ChatMensagem.CriarTexto(Guid.NewGuid(), autorId, "Fábio", "original");
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);

        var command = new EditarMensagemCommand(mensagem.Id, "novo texto", UsuarioId: Guid.NewGuid());

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_QuandoMensagemJaFoiDeletada_DeveLancarBadRequestException()
    {
        var autorId = Guid.NewGuid();
        var mensagem = ChatMensagem.CriarTexto(Guid.NewGuid(), autorId, "Fábio", "original");
        mensagem.Deletar();
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);

        var command = new EditarMensagemCommand(mensagem.Id, "novo texto", UsuarioId: autorId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_QuandoMensagemNaoEhDeTexto_DeveLancarBadRequestException()
    {
        var autorId = Guid.NewGuid();
        var mensagem = ChatMensagem.CriarArquivo(Guid.NewGuid(), autorId, "Fábio", "doc.pdf", "caminho", "application/pdf", 100);
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);

        var command = new EditarMensagemCommand(mensagem.Id, "novo texto", UsuarioId: autorId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    // AC-21: só pode editar até 24h depois do envio.
    [Fact]
    public async Task Handle_QuandoPassaramMaisDe24Horas_DeveLancarBadRequestException()
    {
        var autorId = Guid.NewGuid();
        var mensagem = ChatMensagem.CriarTexto(Guid.NewGuid(), autorId, "Fábio", "original");
        mensagem.DataCriacao = DateTime.UtcNow.AddHours(-25);
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);

        var command = new EditarMensagemCommand(mensagem.Id, "novo texto", UsuarioId: autorId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_DentroDoPrazoDe24Horas_DeveEditarComSucesso()
    {
        var autorId = Guid.NewGuid();
        var mensagem = ChatMensagem.CriarTexto(Guid.NewGuid(), autorId, "Fábio", "original");
        mensagem.DataCriacao = DateTime.UtcNow.AddHours(-23);
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);

        var command = new EditarMensagemCommand(mensagem.Id, "texto editado", UsuarioId: autorId, UsuarioNome: "Fábio");
        await _handler.Handle(command, CancellationToken.None);

        mensagem.Conteudo.Should().Be("texto editado");
        mensagem.EditadaEm.Should().NotBeNull();
        _mensagemRepositoryMock.Verify(r => r.AtualizarAsync(mensagem, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DevePreservarConteudoOriginalNoDetalheDoHistorico()
    {
        // Sem acento de propósito: System.Text.Json escapa unicode por padrão (ú etc.),
        // então um Contains() simples com acento não bateria com o JSON serializado.
        var autorId = Guid.NewGuid();
        var mensagem = ChatMensagem.CriarTexto(Guid.NewGuid(), autorId, "Fabio", "texto original");
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);

        var command = new EditarMensagemCommand(mensagem.Id, "texto novo", UsuarioId: autorId, UsuarioNome: "Fabio");
        await _handler.Handle(command, CancellationToken.None);

        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatHistorico>(h => h.Acao == ChatAcao.MensagemEditada
                && h.Detalhe != null
                && h.Detalhe.Contains("texto original")
                && h.Detalhe.Contains("texto novo")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DevePublicarNotificacaoDeMensagemEditada()
    {
        var autorId = Guid.NewGuid();
        var conversaId = Guid.NewGuid();
        var mensagem = ChatMensagem.CriarTexto(conversaId, autorId, "Fábio", "original");
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);

        var command = new EditarMensagemCommand(mensagem.Id, "editado", UsuarioId: autorId, UsuarioNome: "Fábio");
        await _handler.Handle(command, CancellationToken.None);

        _mediatorMock.Verify(m => m.Publish(
            It.Is<ChatMensagemEditadaNotification>(n => n.ConversaId == conversaId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
