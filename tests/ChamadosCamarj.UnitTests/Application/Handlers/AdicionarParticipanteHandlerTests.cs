using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.Commands.AdicionarParticipante;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class AdicionarParticipanteHandlerTests
{
    private readonly Mock<IChatConversaRepository> _conversaRepositoryMock = new();
    private readonly Mock<IUsuarioPerfilRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IChatMensagemRepository> _mensagemRepositoryMock = new();
    private readonly Mock<IChatHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly AdicionarParticipanteCommandHandler _handler;

    public AdicionarParticipanteHandlerTests()
    {
        _handler = new AdicionarParticipanteCommandHandler(
            _conversaRepositoryMock.Object,
            _usuarioRepositoryMock.Object,
            _mensagemRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _mediatorMock.Object);
    }

    private static UsuarioPerfil CriarUsuario(string nome, ChatPerfil chatPerfil = ChatPerfil.Participante)
    {
        var usuario = new UsuarioPerfil($"{nome.ToLowerInvariant()}@camarj.com.br", nome, Perfil.Atendente);
        usuario.DefinirChatPerfil(chatPerfil);
        return usuario;
    }

    [Fact]
    public async Task Handle_QuandoConversaNaoExiste_DeveLancarNotFoundException()
    {
        var conversaId = Guid.NewGuid();
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(conversaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatConversa?)null);

        var command = new AdicionarParticipanteCommand(conversaId, Guid.NewGuid());

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoConversaNaoEhGrupo_DeveLancarBadRequestException()
    {
        var criadorId = Guid.NewGuid();
        var conversaPrivada = ChatConversa.CriarPrivada(criadorId);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(conversaPrivada.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversaPrivada);

        var command = new AdicionarParticipanteCommand(conversaPrivada.Id, Guid.NewGuid(), criadorId, "Criador", "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_QuandoRequisitanteNaoEhCriadorNemAdmin_DeveLancarForbiddenException()
    {
        var criadorId = Guid.NewGuid();
        var grupo = ChatConversa.CriarGrupo("Grupo", criadorId);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(grupo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);

        var command = new AdicionarParticipanteCommand(grupo.Id, Guid.NewGuid(), Guid.NewGuid(), "Outra Pessoa", "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();

        _usuarioRepositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoRequisitanteEhAdminMasNaoOCriador_DevePermitirAdicionar()
    {
        var criadorId = Guid.NewGuid();
        var grupo = ChatConversa.CriarGrupo("Grupo", criadorId);
        var novoParticipante = CriarUsuario("Mathes");

        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(grupo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(novoParticipante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(novoParticipante);

        var command = new AdicionarParticipanteCommand(grupo.Id, novoParticipante.Id, Guid.NewGuid(), "Um Admin", "Admin");
        await _handler.Handle(command, CancellationToken.None);

        grupo.Participantes.Should().Contain(p => p.UsuarioId == novoParticipante.Id);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioAlvoNaoExiste_DeveLancarNotFoundException()
    {
        var criadorId = Guid.NewGuid();
        var grupo = ChatConversa.CriarGrupo("Grupo", criadorId);
        var alvoId = Guid.NewGuid();

        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(grupo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(alvoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var command = new AdicionarParticipanteCommand(grupo.Id, alvoId, criadorId, "Criador", "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoUsuarioAlvoSemAcessoAoChat_DeveLancarBadRequestException()
    {
        var criadorId = Guid.NewGuid();
        var grupo = ChatConversa.CriarGrupo("Grupo", criadorId);
        var alvo = CriarUsuario("Sem Acesso", ChatPerfil.SemAcesso);

        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(grupo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(alvo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alvo);

        var command = new AdicionarParticipanteCommand(grupo.Id, alvo.Id, criadorId, "Criador", "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_QuandoUsuarioJaEhParticipanteAtivo_DeveLancarConflictException()
    {
        var criadorId = Guid.NewGuid();
        var grupo = ChatConversa.CriarGrupo("Grupo", criadorId);
        var jaParticipante = CriarUsuario("Mathes");
        grupo.AdicionarParticipante(new ChatParticipante(grupo.Id, jaParticipante.Id, jaParticipante.Nome));

        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(grupo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(jaParticipante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jaParticipante);

        var command = new AdicionarParticipanteCommand(grupo.Id, jaParticipante.Id, criadorId, "Criador", "Atendente");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ConflictException>();
    }

    // Alguém que já foi participante e saiu (Ativo = false) não deve gerar linha duplicada —
    // reativa o registro existente.
    [Fact]
    public async Task Handle_QuandoUsuarioFoiParticipanteEEstaInativo_DeveReativarSemDuplicarLinha()
    {
        var criadorId = Guid.NewGuid();
        var grupo = ChatConversa.CriarGrupo("Grupo", criadorId);
        var exParticipante = CriarUsuario("Mathes");
        var participanteInativo = new ChatParticipante(grupo.Id, exParticipante.Id, exParticipante.Nome);
        participanteInativo.Desativar();
        grupo.AdicionarParticipante(participanteInativo);

        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(grupo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(exParticipante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exParticipante);

        var command = new AdicionarParticipanteCommand(grupo.Id, exParticipante.Id, criadorId, "Criador", "Atendente");
        await _handler.Handle(command, CancellationToken.None);

        grupo.Participantes.Should().HaveCount(1);
        grupo.Participantes.Single().Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveAdicionarNovoParticipanteRegistrarHistoricoEPublicarNotificacoes()
    {
        var criadorId = Guid.NewGuid();
        var grupo = ChatConversa.CriarGrupo("Grupo", criadorId);
        var novoParticipante = CriarUsuario("Mathes");

        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(grupo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grupo);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(novoParticipante.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(novoParticipante);

        var command = new AdicionarParticipanteCommand(grupo.Id, novoParticipante.Id, criadorId, "Criador", "Atendente");
        await _handler.Handle(command, CancellationToken.None);

        grupo.Participantes.Should().Contain(p => p.UsuarioId == novoParticipante.Id && p.Ativo);
        _conversaRepositoryMock.Verify(r => r.AtualizarAsync(grupo, It.IsAny<CancellationToken>()), Times.Once);

        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatHistorico>(h => h.Acao == ChatAcao.ParticipanteAdicionado),
            It.IsAny<CancellationToken>()), Times.Once);

        _mensagemRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatMensagem>(m => m.Tipo == ChatMensagemTipo.Sistema),
            It.IsAny<CancellationToken>()), Times.Once);

        _mediatorMock.Verify(m => m.Publish(It.IsAny<ChatNovaMensagemNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(
            It.Is<ChatParticipanteAdicionadoNotification>(n => n.ConversaId == grupo.Id && n.UsuarioId == novoParticipante.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
