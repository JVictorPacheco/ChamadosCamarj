using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Application.Features.Chat.Commands.CriarGrupo;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class CriarGrupoHandlerTests
{
    private readonly Mock<IChatConversaRepository> _conversaRepositoryMock = new();
    private readonly Mock<IUsuarioPerfilRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IChatHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly CriarGrupoCommandHandler _handler;

    public CriarGrupoHandlerTests()
    {
        _handler = new CriarGrupoCommandHandler(
            _conversaRepositoryMock.Object,
            _usuarioRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _mediatorMock.Object);
    }

    private static UsuarioPerfil CriarUsuario(string nome, string email, ChatPerfil chatPerfil)
    {
        var usuario = new UsuarioPerfil(email, nome, Perfil.Atendente);
        usuario.DefinirChatPerfil(chatPerfil);
        return usuario;
    }

    [Fact]
    public async Task Handle_QuandoCriadorNaoTemPermissaoDeCriarGrupo_DeveLancarForbiddenException()
    {
        var criador = CriarUsuario("Fábio", "fabio@camarj.com.br", ChatPerfil.Participante);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(criador.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(criador);

        var command = new CriarGrupoCommand("Grupo Teste", new[] { Guid.NewGuid(), Guid.NewGuid() }, criador.Id, criador.Nome);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_QuandoCriadorNaoExiste_DeveLancarNotFoundException()
    {
        var criadorId = Guid.NewGuid();
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(criadorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var command = new CriarGrupoCommand("Grupo Teste", new[] { Guid.NewGuid(), Guid.NewGuid() }, criadorId, "Fábio");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ComMenosDeDoisParticipantesAlemDoCriador_DeveLancarBadRequestException()
    {
        var criador = CriarUsuario("Fábio", "fabio@camarj.com.br", ChatPerfil.CriadorDeGrupo);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(criador.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(criador);

        var command = new CriarGrupoCommand("Grupo Teste", new[] { Guid.NewGuid() }, criador.Id, criador.Nome);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_DeveIgnorarOProprioCriadorSeEleVierNaListaDeParticipantes()
    {
        // Um grupo precisa de 2 participantes além do criador — se o front mandar o próprio ID
        // do criador na lista (redundante), isso não deveria contar pra atingir o mínimo.
        var criador = CriarUsuario("Fábio", "fabio@camarj.com.br", ChatPerfil.CriadorDeGrupo);
        var outroId = Guid.NewGuid();
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(criador.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(criador);

        var command = new CriarGrupoCommand("Grupo Teste", new[] { criador.Id, outroId }, criador.Id, criador.Nome);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_QuandoParticipanteNaoExiste_DeveLancarNotFoundException()
    {
        var criador = CriarUsuario("Fábio", "fabio@camarj.com.br", ChatPerfil.CriadorDeGrupo);
        var participanteInexistenteId = Guid.NewGuid();
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(criador.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(criador);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(participanteInexistenteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var command = new CriarGrupoCommand("Grupo Teste", new[] { participanteInexistenteId, Guid.NewGuid() }, criador.Id, criador.Nome);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoParticipanteSemAcessoAoChat_DeveLancarBadRequestException()
    {
        var criador = CriarUsuario("Fábio", "fabio@camarj.com.br", ChatPerfil.CriadorDeGrupo);
        var semAcesso = CriarUsuario("Cátia", "catia@camarj.com.br", ChatPerfil.SemAcesso);
        var outro = CriarUsuario("Mathes", "mathes@camarj.com.br", ChatPerfil.Participante);

        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(criador.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(criador);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(semAcesso.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(semAcesso);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(outro.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(outro);

        var command = new CriarGrupoCommand("Grupo Teste", new[] { semAcesso.Id, outro.Id }, criador.Id, criador.Nome);

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveCriarGrupoComCriadorEParticipantesEPublicarNotificacaoParaTodos()
    {
        var criador = CriarUsuario("Fábio", "fabio@camarj.com.br", ChatPerfil.CriadorDeGrupo);
        var participante1 = CriarUsuario("Mathes", "mathes@camarj.com.br", ChatPerfil.Participante);
        var participante2 = CriarUsuario("Cátia", "catia@camarj.com.br", ChatPerfil.Participante);

        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(criador.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(criador);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(participante1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participante1);
        _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(participante2.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participante2);

        var command = new CriarGrupoCommand("Equipe de Atendimento", new[] { participante1.Id, participante2.Id }, criador.Id, criador.Nome);
        var response = await _handler.Handle(command, CancellationToken.None);

        response.Nome.Should().Be("Equipe de Atendimento");
        response.Tipo.Should().Be(ChatConversaTipo.Grupo);

        _conversaRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatConversa>(c => c.Participantes.Count == 3
                && c.Participantes.Any(p => p.UsuarioId == criador.Id)
                && c.Participantes.Any(p => p.UsuarioId == participante1.Id)
                && c.Participantes.Any(p => p.UsuarioId == participante2.Id)),
            It.IsAny<CancellationToken>()), Times.Once);

        _historicoRepositoryMock.Verify(r => r.AdicionarAsync(
            It.Is<ChatHistorico>(h => h.Acao == ChatAcao.GrupoCriado),
            It.IsAny<CancellationToken>()), Times.Once);

        _mediatorMock.Verify(m => m.Publish(
            It.Is<ChatNovaConversaNotification>(n =>
                n.ParticipanteIds.Contains(criador.Id) &&
                n.ParticipanteIds.Contains(participante1.Id) &&
                n.ParticipanteIds.Contains(participante2.Id)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
