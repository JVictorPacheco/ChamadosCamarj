using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chat.Queries.ListarHistoricoChat;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class ChatHistoricoHandlerTests
{
    private readonly Mock<IChatHistoricoRepository> _historicoRepositoryMock = new();
    private readonly ListarHistoricoChatQueryHandler _handler;

    public ChatHistoricoHandlerTests()
    {
        _handler = new ListarHistoricoChatQueryHandler(_historicoRepositoryMock.Object);
    }

    // AC-36: só Admin acessa os logs de chat.
    [Fact]
    public async Task Handle_QuandoRequisitanteNaoEhAdmin_DeveLancarForbiddenException()
    {
        var query = new ListarHistoricoChatQuery(PerfilRequisitante: "Atendente");

        var act = async () => await _handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();

        _historicoRepositoryMock.Verify(r => r.ListarTodasAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SemFiltroDeConversa_DeveListarTodoOHistorico()
    {
        var historicos = new[]
        {
            ChatHistorico.Criar(Guid.NewGuid(), "Fabio", ChatAcao.MensagemEnviada),
            ChatHistorico.Criar(Guid.NewGuid(), "Admin", ChatAcao.AcessoRevogado),
        };
        _historicoRepositoryMock.Setup(r => r.ListarTodasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(historicos);

        var query = new ListarHistoricoChatQuery(PerfilRequisitante: "Admin");
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        result.Should().HaveCount(2);
        _historicoRepositoryMock.Verify(r => r.ListarPorConversaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComFiltroDeConversa_DeveListarSoOHistoricoDaquelaConversa()
    {
        var conversaId = Guid.NewGuid();
        var historicos = new[] { ChatHistorico.Criar(Guid.NewGuid(), "Fabio", ChatAcao.MensagemEnviada, null, conversaId) };
        _historicoRepositoryMock.Setup(r => r.ListarPorConversaAsync(conversaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(historicos);

        var query = new ListarHistoricoChatQuery(conversaId, "Admin");
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        result.Should().HaveCount(1);
        _historicoRepositoryMock.Verify(r => r.ListarTodasAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveMapearCamposDoHistoricoCorretamente()
    {
        var usuarioId = Guid.NewGuid();
        var conversaId = Guid.NewGuid();
        var mensagemId = Guid.NewGuid();
        var historico = ChatHistorico.Criar(usuarioId, "Fabio", ChatAcao.MensagemDeletada, "detalhe", conversaId, mensagemId);

        _historicoRepositoryMock.Setup(r => r.ListarTodasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { historico });

        var query = new ListarHistoricoChatQuery(PerfilRequisitante: "Admin");
        var result = (await _handler.Handle(query, CancellationToken.None)).Single();

        result.UsuarioId.Should().Be(usuarioId);
        result.UsuarioNome.Should().Be("Fabio");
        result.Acao.Should().Be(ChatAcao.MensagemDeletada);
        result.Detalhe.Should().Be("detalhe");
        result.ConversaId.Should().Be(conversaId);
        result.MensagemId.Should().Be(mensagemId);
    }
}
