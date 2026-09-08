using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chat.Queries.ObterConversa;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

// review-fase9-independente.md #2 — ver mesmo comentário em ListarConversasHandlerTests.cs.
// AC-53 a AC-58 (2026-09-08): read receipts com reciprocidade (design.md, chat-corporativo, seção 10.2).
public class ObterConversaHandlerTests
{
    private readonly Mock<IChatConversaRepository> _conversaRepositoryMock = new();
    private readonly Mock<IUsuarioPerfilRepository> _usuarioPerfilRepositoryMock = new();
    private readonly ObterConversaQueryHandler _handler;

    public ObterConversaHandlerTests()
    {
        _handler = new ObterConversaQueryHandler(_conversaRepositoryMock.Object, _usuarioPerfilRepositoryMock.Object);
    }

    private static UsuarioPerfil CriarUsuario(bool mostrarConfirmacaoLeitura = true)
    {
        var usuario = new UsuarioPerfil($"{Guid.NewGuid()}@camarj.com.br", "Usuário Teste", Perfil.Atendente);
        usuario.DefinirPreferenciaLeitura(mostrarConfirmacaoLeitura);
        return usuario;
    }

    private static ChatParticipante CriarParticipante(Guid conversaId, Guid usuarioId, string nome, DateTime? ultimaLeituraEm)
    {
        var participante = new ChatParticipante(conversaId, usuarioId, nome);
        if (ultimaLeituraEm is not null)
            participante.MarcarComoLido();
        return participante;
    }

    [Fact]
    public async Task Handle_QuandoUsuarioSemAcesso_DeveLancarForbiddenException()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var query = new ObterConversaQuery(Guid.NewGuid(), usuario.Id);
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _conversaRepositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioComAcessoRevogadoContinuaParticipante_AindaAssimDeveBloquear()
    {
        // Confirma exatamente o cenário do achado #2: revogar não remove o vínculo de participante,
        // então sem a guarda por ChatPerfil essa pessoa continuaria conseguindo ler a conversa.
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        // ChatPerfil já nasce SemAcesso — não precisa conceder e revogar, só confirma o estado.

        var conversa = ChatConversa.CriarGrupo("Equipe", Guid.NewGuid());
        conversa.AdicionarParticipante(new ChatParticipante(conversa.Id, usuario.Id, usuario.Nome));

        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(conversa.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversa);

        var query = new ObterConversaQuery(conversa.Id, usuario.Id);
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_QuandoUsuarioNaoExiste_DeveLancarNotFoundException()
    {
        var usuarioId = Guid.NewGuid();
        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var query = new ObterConversaQuery(Guid.NewGuid(), usuarioId);
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_QuandoUsuarioComAcessoEParticipante_DeveRetornarDetalhe()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.DefinirChatPerfil(ChatPerfil.Participante);

        var conversa = ChatConversa.CriarGrupo("Equipe", Guid.NewGuid());
        conversa.AdicionarParticipante(new ChatParticipante(conversa.Id, usuario.Id, usuario.Nome));

        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(conversa.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversa);

        var query = new ObterConversaQuery(conversa.Id, usuario.Id);
        var resultado = await _handler.Handle(query, CancellationToken.None);

        resultado.Id.Should().Be(conversa.Id);
    }

    [Fact]
    public async Task Handle_QuandoAmbosOsToggleEstaoLigados_DeveExporUltimaLeituraDoOutroParticipante()
    {
        var euUsuario = CriarUsuario(mostrarConfirmacaoLeitura: true);
        euUsuario.DefinirChatPerfil(ChatPerfil.Participante);
        var outroUsuario = CriarUsuario(mostrarConfirmacaoLeitura: true);

        var conversa = ChatConversa.CriarPrivada(euUsuario.Id);
        conversa.AdicionarParticipante(CriarParticipante(conversa.Id, euUsuario.Id, euUsuario.Nome, ultimaLeituraEm: null));
        conversa.AdicionarParticipante(CriarParticipante(conversa.Id, outroUsuario.Id, outroUsuario.Nome, ultimaLeituraEm: DateTime.UtcNow));

        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(euUsuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(euUsuario);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(conversa.Id, It.IsAny<CancellationToken>())).ReturnsAsync(conversa);
        _usuarioPerfilRepositoryMock.Setup(r => r.ListarPorIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { euUsuario, outroUsuario });

        var result = await _handler.Handle(new ObterConversaQuery(conversa.Id, euUsuario.Id), CancellationToken.None);

        result.Participantes.Single(p => p.UsuarioId == outroUsuario.Id).UltimaLeituraEm.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_QuandoMeuProprioToggleEstaDesligado_NaoDeveExporLeituraDeNinguem()
    {
        var euUsuario = CriarUsuario(mostrarConfirmacaoLeitura: false);
        euUsuario.DefinirChatPerfil(ChatPerfil.Participante);
        var outroUsuario = CriarUsuario(mostrarConfirmacaoLeitura: true);

        var conversa = ChatConversa.CriarPrivada(euUsuario.Id);
        conversa.AdicionarParticipante(CriarParticipante(conversa.Id, euUsuario.Id, euUsuario.Nome, ultimaLeituraEm: null));
        conversa.AdicionarParticipante(CriarParticipante(conversa.Id, outroUsuario.Id, outroUsuario.Nome, ultimaLeituraEm: DateTime.UtcNow));

        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(euUsuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(euUsuario);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(conversa.Id, It.IsAny<CancellationToken>())).ReturnsAsync(conversa);
        _usuarioPerfilRepositoryMock.Setup(r => r.ListarPorIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { euUsuario, outroUsuario });

        var result = await _handler.Handle(new ObterConversaQuery(conversa.Id, euUsuario.Id), CancellationToken.None);

        result.Participantes.Single(p => p.UsuarioId == outroUsuario.Id).UltimaLeituraEm.Should().BeNull();
    }

    [Fact]
    public async Task Handle_QuandoToggleDoOutroParticipanteEstaDesligado_NaoDeveExporALeituraDele_MasExpoeADosDemais()
    {
        var euUsuario = CriarUsuario(mostrarConfirmacaoLeitura: true);
        euUsuario.DefinirChatPerfil(ChatPerfil.Participante);
        var participanteComToggleDesligado = CriarUsuario(mostrarConfirmacaoLeitura: false);
        var participanteComToggleLigado = CriarUsuario(mostrarConfirmacaoLeitura: true);

        var grupo = ChatConversa.CriarGrupo("Grupo Teste", euUsuario.Id);
        grupo.AdicionarParticipante(CriarParticipante(grupo.Id, euUsuario.Id, euUsuario.Nome, ultimaLeituraEm: null));
        grupo.AdicionarParticipante(CriarParticipante(grupo.Id, participanteComToggleDesligado.Id, participanteComToggleDesligado.Nome, ultimaLeituraEm: DateTime.UtcNow));
        grupo.AdicionarParticipante(CriarParticipante(grupo.Id, participanteComToggleLigado.Id, participanteComToggleLigado.Nome, ultimaLeituraEm: DateTime.UtcNow));

        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(euUsuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(euUsuario);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(grupo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(grupo);
        _usuarioPerfilRepositoryMock.Setup(r => r.ListarPorIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { euUsuario, participanteComToggleDesligado, participanteComToggleLigado });

        var result = await _handler.Handle(new ObterConversaQuery(grupo.Id, euUsuario.Id), CancellationToken.None);

        result.Participantes.Single(p => p.UsuarioId == participanteComToggleDesligado.Id).UltimaLeituraEm.Should().BeNull();
        result.Participantes.Single(p => p.UsuarioId == participanteComToggleLigado.Id).UltimaLeituraEm.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_QuandoAmbosOsTogglesEstaoDesligados_NaoDeveExporLeitura()
    {
        var euUsuario = CriarUsuario(mostrarConfirmacaoLeitura: false);
        euUsuario.DefinirChatPerfil(ChatPerfil.Participante);
        var outroUsuario = CriarUsuario(mostrarConfirmacaoLeitura: false);

        var conversa = ChatConversa.CriarPrivada(euUsuario.Id);
        conversa.AdicionarParticipante(CriarParticipante(conversa.Id, euUsuario.Id, euUsuario.Nome, ultimaLeituraEm: null));
        conversa.AdicionarParticipante(CriarParticipante(conversa.Id, outroUsuario.Id, outroUsuario.Nome, ultimaLeituraEm: DateTime.UtcNow));

        _usuarioPerfilRepositoryMock.Setup(r => r.ObterPorIdAsync(euUsuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(euUsuario);
        _conversaRepositoryMock.Setup(r => r.ObterPorIdAsync(conversa.Id, It.IsAny<CancellationToken>())).ReturnsAsync(conversa);
        _usuarioPerfilRepositoryMock.Setup(r => r.ListarPorIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { euUsuario, outroUsuario });

        var result = await _handler.Handle(new ObterConversaQuery(conversa.Id, euUsuario.Id), CancellationToken.None);

        result.Participantes.Single(p => p.UsuarioId == outroUsuario.Id).UltimaLeituraEm.Should().BeNull();
    }
}
