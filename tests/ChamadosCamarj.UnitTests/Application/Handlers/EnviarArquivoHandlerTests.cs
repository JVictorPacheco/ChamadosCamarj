using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.Commands.EnviarArquivo;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class EnviarArquivoHandlerTests
{
    private readonly Mock<IChatConversaRepository> _conversaRepositoryMock = new();
    private readonly Mock<IChatMensagemRepository> _mensagemRepositoryMock = new();
    private readonly Mock<IChatHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IUsuarioPerfilRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IChatStorageService> _storageServiceMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly EnviarArquivoCommandHandler _handler;

    public EnviarArquivoHandlerTests()
    {
        _handler = new EnviarArquivoCommandHandler(
            _conversaRepositoryMock.Object,
            _mensagemRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _usuarioRepositoryMock.Object,
            _storageServiceMock.Object,
            _mediatorMock.Object);
    }

    private static UsuarioPerfil CriarUsuarioComAcesso(ChatPerfil chatPerfil = ChatPerfil.Participante)
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.DefinirChatPerfil(chatPerfil);
        return usuario;
    }

    private static EnviarArquivoCommand ComandoValido(Guid conversaId, Guid usuarioId, string nome = "Fábio") =>
        new(conversaId, "documento.pdf", "application/pdf", new MemoryStream(new byte[] { 1, 2, 3 }), 3, usuarioId, nome);

    [Fact]
    public async Task Handle_QuandoUsuarioNaoExiste_DeveLancarNotFoundException()
    {
        var usuarioId = Guid.NewGuid();
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var command = ComandoValido(Guid.NewGuid(), usuarioId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoUsuarioSemAcessoAoChat_DeveLancarForbiddenException()
    {
        var usuario = CriarUsuarioComAcesso(ChatPerfil.SemAcesso);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = ComandoValido(Guid.NewGuid(), usuario.Id);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();

        _storageServiceMock.Verify(s => s.UploadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoNaoEhParticipanteDaConversa_DeveLancarForbiddenException()
    {
        var usuario = CriarUsuarioComAcesso();
        var conversaId = Guid.NewGuid();
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ObterParticipanteAsync(conversaId, usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatParticipante?)null);

        var command = ComandoValido(conversaId, usuario.Id);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_ComArquivoValido_DeveFazerUploadRegistrarMensagemEHistoricoEPublicarNotificacao()
    {
        var usuario = CriarUsuarioComAcesso();
        var conversaId = Guid.NewGuid();
        var participante = new ChatParticipante(conversaId, usuario.Id, usuario.Nome);

        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ObterParticipanteAsync(conversaId, usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participante);

        var command = ComandoValido(conversaId, usuario.Id, usuario.Nome);
        var response = await _handler.Handle(command, CancellationToken.None);

        response.NomeArquivo.Should().Be("documento.pdf");
        response.Tipo.Should().Be(ChatMensagemTipo.Arquivo);

        _storageServiceMock.Verify(s => s.UploadAsync(
            It.IsAny<string>(), "application/pdf", It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);
        _mensagemRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<ChatMensagem>(), It.IsAny<CancellationToken>()), Times.Once);
        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatHistorico>(h => h.Acao == ChatAcao.ArquivoEnviado),
            It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(
            It.Is<ChatNovaMensagemNotification>(n => n.ConversaId == conversaId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // Mesmo padrão já usado em AdicionarAnexoCommandHandler (anexos de chamados): se o insert no
    // banco falhar depois do upload já ter subido pro Storage, o arquivo órfão precisa ser removido.
    [Fact]
    public async Task Handle_QuandoInsertNoBancoFalha_DeveRemoverArquivoOrfaoDoStorage()
    {
        var usuario = CriarUsuarioComAcesso();
        var conversaId = Guid.NewGuid();
        var participante = new ChatParticipante(conversaId, usuario.Id, usuario.Nome);

        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ObterParticipanteAsync(conversaId, usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participante);
        _mensagemRepositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<ChatMensagem>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("falha simulada no banco"));

        var command = ComandoValido(conversaId, usuario.Id);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        _storageServiceMock.Verify(s => s.RemoverAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<ChatHistorico>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
