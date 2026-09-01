using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chat.Commands.DefinirChatPerfil;
using ChamadosCamarj.Application.Features.Usuarios.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

public class AtualizarUsuarioPerfilHandlerTests
{
    private readonly Mock<IUsuarioPerfilRepository> _repositoryMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly AtualizarUsuarioPerfilCommandHandler _handler;

    public AtualizarUsuarioPerfilHandlerTests()
    {
        _handler = new AtualizarUsuarioPerfilCommandHandler(
            _repositoryMock.Object,
            _mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_QuandoUsuarioNaoExiste_DeveRetornarNull()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPerfil?)null);

        var command = new AtualizarUsuarioPerfilCommand(id, "Novo Nome", Perfil.Admin, true, ChatPerfil.SemAcesso, PerfilRequisitante: "Admin");
        var response = await _handler.Handle(command, CancellationToken.None);

        response.Should().BeNull();
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioPerfil>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveAtualizarNomeEPerfil()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        _repositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new AtualizarUsuarioPerfilCommand(usuario.Id, "Fábio Silva", Perfil.Admin, true, ChatPerfil.SemAcesso, PerfilRequisitante: "Admin");
        var response = await _handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response!.Nome.Should().Be("Fábio Silva");
        response.Perfil.Should().Be(Perfil.Admin);
        _repositoryMock.Verify(r => r.AtualizarAsync(usuario, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComAtivoFalse_DeveDesativarUsuario()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        _repositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new AtualizarUsuarioPerfilCommand(usuario.Id, "Fábio", Perfil.Atendente, false, ChatPerfil.SemAcesso, PerfilRequisitante: "Admin");
        var response = await _handler.Handle(command, CancellationToken.None);

        response!.Ativo.Should().BeFalse();
        usuario.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ComAtivoTrueParaUsuarioDesativado_DeveReativar()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        usuario.Desativar();
        _repositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new AtualizarUsuarioPerfilCommand(usuario.Id, "Fábio", Perfil.Atendente, true, ChatPerfil.SemAcesso, PerfilRequisitante: "Admin");
        var response = await _handler.Handle(command, CancellationToken.None);

        response!.Ativo.Should().BeTrue();
        usuario.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_QuandoRequisitanteNaoEhAdmin_DeveLancarForbiddenException()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        _repositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new AtualizarUsuarioPerfilCommand(usuario.Id, "Fábio", Perfil.Atendente, true, ChatPerfil.SemAcesso, PerfilRequisitante: "Solicitante");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();

        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioPerfil>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── D-02: guarda contra auto-lockout do último Admin ativo ────────────────

    [Fact]
    public async Task Handle_QuandoEhOUnicoAdminAtivoEDesativado_DeveLancarConflictException()
    {
        var unicoAdmin = new UsuarioPerfil("victor@camarj.com.br", "Victor", Perfil.Admin);
        var atendente = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);

        _repositoryMock.Setup(r => r.ObterPorIdAsync(unicoAdmin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unicoAdmin);
        _repositoryMock.Setup(r => r.ListarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UsuarioPerfil> { unicoAdmin, atendente });

        // Ativo = false enquanto mantém o Perfil Admin
        var command = new AtualizarUsuarioPerfilCommand(unicoAdmin.Id, "Victor", Perfil.Admin, false, ChatPerfil.SemAcesso, PerfilRequisitante: "Admin");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ConflictException>();

        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<UsuarioPerfil>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoEhOUnicoAdminAtivoERebaixado_DeveLancarConflictException()
    {
        var unicoAdmin = new UsuarioPerfil("victor@camarj.com.br", "Victor", Perfil.Admin);

        _repositoryMock.Setup(r => r.ObterPorIdAsync(unicoAdmin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unicoAdmin);
        _repositoryMock.Setup(r => r.ListarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UsuarioPerfil> { unicoAdmin });

        // Continua ativo, mas deixa de ser Admin
        var command = new AtualizarUsuarioPerfilCommand(unicoAdmin.Id, "Victor", Perfil.Atendente, true, ChatPerfil.SemAcesso, PerfilRequisitante: "Admin");

        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_QuandoHaOutroAdminAtivo_DevePermitirDesativarUmDeles()
    {
        var admin1 = new UsuarioPerfil("victor@camarj.com.br", "Victor", Perfil.Admin);
        var admin2 = new UsuarioPerfil("catia@camarj.com.br", "Cátia", Perfil.Admin);

        _repositoryMock.Setup(r => r.ObterPorIdAsync(admin1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin1);
        _repositoryMock.Setup(r => r.ListarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UsuarioPerfil> { admin1, admin2 });

        var command = new AtualizarUsuarioPerfilCommand(admin1.Id, "Victor", Perfil.Admin, false, ChatPerfil.SemAcesso, PerfilRequisitante: "Admin");
        var response = await _handler.Handle(command, CancellationToken.None);

        response!.Ativo.Should().BeFalse();
        _repositoryMock.Verify(r => r.AtualizarAsync(admin1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoAdminInativoEDesativadoDeNovo_NaoDeveContarComoUltimoAdminAtivo()
    {
        // Usuário alvo já está inativo — não é "o último Admin ativo" sendo derrubado,
        // então a guarda de D-02 não deve nem consultar a lista de usuários.
        var adminInativo = new UsuarioPerfil("victor@camarj.com.br", "Victor", Perfil.Admin);
        adminInativo.Desativar();

        _repositoryMock.Setup(r => r.ObterPorIdAsync(adminInativo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminInativo);

        var command = new AtualizarUsuarioPerfilCommand(adminInativo.Id, "Victor", Perfil.Atendente, false, ChatPerfil.SemAcesso, PerfilRequisitante: "Admin");
        var response = await _handler.Handle(command, CancellationToken.None);

        response!.Perfil.Should().Be(Perfil.Atendente);
        _repositoryMock.Verify(r => r.ListarAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── review-fase9-independente.md #1: ChatPerfil despacha DefinirChatPerfilCommand em vez de
    // duplicar auditoria/notificação — cobre que essa tela não voltou a divergir da outra ──────────

    [Fact]
    public async Task Handle_QuandoChatPerfilMuda_DeveDespacharDefinirChatPerfilCommand()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        _repositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new AtualizarUsuarioPerfilCommand(
            usuario.Id, "Fábio", Perfil.Atendente, true, ChatPerfil.Participante,
            PerfilRequisitante: "Admin", RequisitanteId: Guid.NewGuid(), RequisitanteNome: "Admin Teste");
        await _handler.Handle(command, CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(
            It.Is<DefinirChatPerfilCommand>(c =>
                c.UsuarioId == usuario.Id &&
                c.ChatPerfil == ChatPerfil.Participante &&
                c.AdminId == command.RequisitanteId &&
                c.AdminNome == "Admin Teste"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoChatPerfilMuda_RespostaDeveRefletirNovoChatPerfil()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        _repositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new AtualizarUsuarioPerfilCommand(usuario.Id, "Fábio", Perfil.Atendente, true, ChatPerfil.CriadorDeGrupo, PerfilRequisitante: "Admin");
        var response = await _handler.Handle(command, CancellationToken.None);

        response!.ChatPerfil.Should().Be(ChatPerfil.CriadorDeGrupo);
    }

    [Fact]
    public async Task Handle_QuandoChatPerfilNaoMuda_NaoDeveDespacharDefinirChatPerfilCommand()
    {
        var usuario = new UsuarioPerfil("fabio@camarj.com.br", "Fábio", Perfil.Atendente);
        _repositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new AtualizarUsuarioPerfilCommand(usuario.Id, "Fábio", Perfil.Atendente, true, ChatPerfil.SemAcesso, PerfilRequisitante: "Admin");
        await _handler.Handle(command, CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.IsAny<DefinirChatPerfilCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
