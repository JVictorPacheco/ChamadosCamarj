using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.Commands.DeletarMensagem;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class DeletarMensagemHandlerTests
{
    private readonly Mock<IChatMensagemRepository> _mensagemRepositoryMock = new();
    private readonly Mock<IChatHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly DeletarMensagemCommandHandler _handler;

    public DeletarMensagemHandlerTests()
    {
        _handler = new DeletarMensagemCommandHandler(
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

        var command = new DeletarMensagemCommand(mensagemId, Guid.NewGuid(), "Fábio", "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoNaoEhAutorNemAdmin_DeveLancarForbiddenException()
    {
        var autorId = Guid.NewGuid();
        var mensagem = ChatMensagem.CriarTexto(Guid.NewGuid(), autorId, "Fábio", "texto");
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);

        var command = new DeletarMensagemCommand(mensagem.Id, Guid.NewGuid(), "Outra Pessoa", "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    // AC-23: o autor pode deletar a própria mensagem.
    [Fact]
    public async Task Handle_QuandoEhOAutor_DevePermitirDeletar()
    {
        var autorId = Guid.NewGuid();
        var mensagem = ChatMensagem.CriarTexto(Guid.NewGuid(), autorId, "Fábio", "texto");
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);

        var command = new DeletarMensagemCommand(mensagem.Id, autorId, "Fábio", "Atendente");
        await _handler.Handle(command, CancellationToken.None);

        mensagem.Deletada.Should().BeTrue();
        mensagem.Conteudo.Should().BeNull();
        _mensagemRepositoryMock.Verify(r => r.AtualizarAsync(mensagem, It.IsAny<CancellationToken>()), Times.Once);
    }

    // AC-24: um Admin pode deletar qualquer mensagem, mesmo não sendo o autor.
    [Fact]
    public async Task Handle_QuandoEhAdminMasNaoOAutor_DevePermitirDeletar()
    {
        var autorId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var mensagem = ChatMensagem.CriarTexto(Guid.NewGuid(), autorId, "Fábio", "texto");
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);

        var command = new DeletarMensagemCommand(mensagem.Id, adminId, "Admin", "Admin");
        await _handler.Handle(command, CancellationToken.None);

        mensagem.Deletada.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_QuandoJaEstaDeletada_NaoDeveGerarHistoricoNemNotificacao()
    {
        var autorId = Guid.NewGuid();
        var mensagem = ChatMensagem.CriarTexto(Guid.NewGuid(), autorId, "Fábio", "texto");
        mensagem.Deletar();
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);

        var command = new DeletarMensagemCommand(mensagem.Id, autorId, "Fábio", "Atendente");
        await _handler.Handle(command, CancellationToken.None);

        _mensagemRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<ChatMensagem>(), It.IsAny<CancellationToken>()), Times.Never);
        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<ChatHistorico>(), It.IsAny<CancellationToken>()), Times.Never);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // AC-25: o conteúdo original fica preservado no log de auditoria, mesmo depois de apagado da mensagem.
    [Fact]
    public async Task Handle_DevePreservarConteudoOriginalNoDetalheDoHistorico()
    {
        // Sem acento de propósito: System.Text.Json escapa unicode por padrão.
        var autorId = Guid.NewGuid();
        var mensagem = ChatMensagem.CriarTexto(Guid.NewGuid(), autorId, "Fabio", "texto sensivel");
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);

        var command = new DeletarMensagemCommand(mensagem.Id, autorId, "Fabio", "Atendente");
        await _handler.Handle(command, CancellationToken.None);

        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatHistorico>(h => h.Acao == ChatAcao.MensagemDeletada
                && h.Detalhe != null
                && h.Detalhe.Contains("texto sensivel")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoAdminDeletaMensagemDeOutro_DeveMarcarDeletadaPorAdminNoHistorico()
    {
        var autorId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var mensagem = ChatMensagem.CriarTexto(Guid.NewGuid(), autorId, "Fábio", "texto");
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);

        var command = new DeletarMensagemCommand(mensagem.Id, adminId, "Admin", "Admin");
        await _handler.Handle(command, CancellationToken.None);

        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatHistorico>(h => h.Detalhe != null && h.Detalhe.Contains("\"deletadaPorAdmin\":true")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DevePublicarNotificacaoDeMensagemDeletada()
    {
        var autorId = Guid.NewGuid();
        var conversaId = Guid.NewGuid();
        var mensagem = ChatMensagem.CriarTexto(conversaId, autorId, "Fábio", "texto");
        _mensagemRepositoryMock.Setup(r => r.ObterPorIdAsync(mensagem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mensagem);

        var command = new DeletarMensagemCommand(mensagem.Id, autorId, "Fábio", "Atendente");
        await _handler.Handle(command, CancellationToken.None);

        _mediatorMock.Verify(m => m.Publish(
            It.Is<ChatMensagemDeletadaNotification>(n => n.ConversaId == conversaId && n.MensagemId == mensagem.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
