using ChamadosCamarj.Application.Common.Autorizacao;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Behaviours;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Application.Features.Chamados.Queries;
using ChamadosCamarj.Domain.Common;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChamadosCamarj.UnitTests.Application.Autorizacao;

/// <summary>
/// Pipeline de acesso a chamados — spec autorizacao-chamados (AC-03, AC-05, AC-06, AC-10, AC-16).
/// </summary>
public class AcessoChamadoBehaviourTests
{
    private readonly Mock<IChamadoRepository> _repositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Guid _chamadoId = Guid.NewGuid();
    private bool _handlerChamado;

    private void Usuario(string perfil, string email = "usuario@camarj.com.br")
    {
        _currentUserMock.SetupGet(c => c.Perfil).Returns(perfil);
        _currentUserMock.SetupGet(c => c.Email).Returns(email);
        _currentUserMock.SetupGet(c => c.UsuarioId).Returns(Guid.NewGuid());
    }

    private void PodeVer(bool podeVer) =>
        _repositoryMock.Setup(r => r.PodeVerAsync(_chamadoId, It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(podeVer);

    private Task<TResponse> Executar<TRequest, TResponse>(TRequest request, TResponse resposta) where TRequest : notnull
    {
        var behaviour = new AcessoChamadoBehaviour<TRequest, TResponse>(_repositoryMock.Object, _currentUserMock.Object);
        return behaviour.Handle(request, _ => { _handlerChamado = true; return Task.FromResult(resposta); }, CancellationToken.None);
    }

    [Fact]
    public async Task RequestQueNaoEhDeChamado_PassaDiretoSemConsultarOBanco()
    {
        Usuario("Solicitante");

        await Executar(new ListarChamadosQuery(), default(object)!);

        _handlerChamado.Should().BeTrue();
        _repositoryMock.Verify(r => r.PodeVerAsync(It.IsAny<Guid>(), It.IsAny<ContextoAcesso>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChamadoQueNaoPodeVer_DaNotFound_ENaoChamaOHandler()
    {
        Usuario("Solicitante");
        PodeVer(false);

        var act = () => Executar(new ObterChamadoPorIdQuery(_chamadoId), default(object)!);

        await act.Should().ThrowAsync<NotFoundException>();   // AC-16: não revela que existe
        _handlerChamado.Should().BeFalse();
    }

    [Fact]
    public async Task ChamadoQuePodeVer_SemPermissaoParaAcao_DaForbidden_ENaoChamaOHandler()
    {
        Usuario("Solicitante");
        PodeVer(true);

        var act = () => Executar(new ResolverChamadoCommand(_chamadoId), Unit.Value);

        await act.Should().ThrowAsync<ForbiddenException>();
        _handlerChamado.Should().BeFalse();
    }

    [Fact]
    public async Task ChamadoQuePodeVer_ComPermissao_ChamaOHandler()
    {
        Usuario("Atendente");
        PodeVer(true);

        await Executar(new ResolverChamadoCommand(_chamadoId), Unit.Value);

        _handlerChamado.Should().BeTrue();
    }

    [Fact]
    public async Task Solicitante_ComentarioInterno_DaForbidden()
    {
        Usuario("Solicitante");
        PodeVer(true);

        var act = () => Executar(new ComentarChamadoCommand(_chamadoId, "Ana", "texto", Interno: true), default(object)!);

        await act.Should().ThrowAsync<ForbiddenException>();   // AC-10
    }

    [Fact]
    public async Task Solicitante_CancelaOProprioChamado()
    {
        Usuario("Solicitante", email: "ana@camarj.com.br");
        PodeVer(true);
        _repositoryMock.Setup(r => r.ObterPorIdAsync(_chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Chamado("T", "D", "Ana", "ana@camarj.com.br", Guid.NewGuid(), Guid.NewGuid()));

        await Executar(new CancelarChamadoCommand(_chamadoId, MotivoEncerramento.CanceladoSolicitante), Unit.Value);

        _handlerChamado.Should().BeTrue();
    }

    [Fact]
    public async Task Solicitante_DoGrupo_NaoCancelaChamadoDoColega()
    {
        Usuario("Solicitante", email: "bruno@camarj.com.br");
        PodeVer(true);   // vê porque é do grupo (AC-02)...
        _repositoryMock.Setup(r => r.ObterPorIdAsync(_chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Chamado("T", "D", "Ana", "ana@camarj.com.br", Guid.NewGuid(), Guid.NewGuid()));

        var act = () => Executar(new CancelarChamadoCommand(_chamadoId, MotivoEncerramento.CanceladoSolicitante), Unit.Value);

        await act.Should().ThrowAsync<ForbiddenException>();   // ...mas não cancela (AC-06)
    }

    [Fact]
    public async Task DownloadDeAnexo_ChecaOChamadoDaUrl()
    {
        Usuario("Solicitante");
        PodeVer(false);

        var act = () => Executar(new ObterUrlDownloadAnexoQuery(_chamadoId, Guid.NewGuid()), "url");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── Editar (spec editar-chamado) ─────────────────────────────────────────

    private Chamado ChamadoNoBanco(string emailDeQuemAbriu = "ana@camarj.com.br")
    {
        var chamado = new Chamado("T", "D", "Ana", emailDeQuemAbriu, Guid.NewGuid(), Guid.NewGuid());
        _repositoryMock.Setup(r => r.ObterPorIdAsync(_chamadoId, It.IsAny<CancellationToken>())).ReturnsAsync(chamado);
        return chamado;
    }

    [Fact]
    public async Task Editar_ChamadoEncerrado_DaBadRequestComAMensagem_InclusiveParaAdmin()
    {
        // AC-10
        Usuario("Admin");
        PodeVer(true);
        var chamado = ChamadoNoBanco();
        chamado.Atribuir(Guid.NewGuid(), "Atendente");
        chamado.Resolver();

        var act = () => Executar(new AtualizarChamadoCommand(_chamadoId, "Novo", "Nova"), Unit.Value);

        await act.Should().ThrowAsync<BadRequestException>().WithMessage(ChamadoPermissoes.MensagemEdicaoEncerrado);
        _handlerChamado.Should().BeFalse();
    }

    [Fact]
    public async Task Editar_AtendenteQueNaoEhResponsavel_DaForbidden()
    {
        // AC-06
        Usuario("Atendente", email: "b@camarj.com.br");
        PodeVer(true);
        ChamadoNoBanco().Atribuir(Guid.NewGuid(), "Atendente A");

        var act = () => Executar(new AtualizarChamadoCommand(_chamadoId, "Novo", "Nova"), Unit.Value);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Editar_ResponsavelAtual_ChamaOHandler()
    {
        // AC-05
        var atendenteId = Guid.NewGuid();
        Usuario("Atendente", email: "a@camarj.com.br");
        _currentUserMock.SetupGet(c => c.UsuarioId).Returns(atendenteId);
        PodeVer(true);
        ChamadoNoBanco().Atribuir(atendenteId, "Atendente A");

        await Executar(new AtualizarChamadoCommand(_chamadoId, "Novo", "Nova"), Unit.Value);

        _handlerChamado.Should().BeTrue();
    }

    [Fact]
    public async Task Editar_QuemAbriuSemResponsavel_ChamaOHandler()
    {
        // AC-01
        Usuario("Solicitante", email: "ana@camarj.com.br");
        PodeVer(true);
        ChamadoNoBanco("ana@camarj.com.br");

        await Executar(new AtualizarChamadoCommand(_chamadoId, "Novo", "Nova"), Unit.Value);

        _handlerChamado.Should().BeTrue();
    }

    [Fact]
    public async Task AcoesQueNaoDependemDoChamado_NaoFazemConsultaExtra()
    {
        // Ponto de toque cross-feature: a mudança não pode encarecer as outras ações.
        Usuario("Atendente");
        PodeVer(true);

        await Executar(new ResolverChamadoCommand(_chamadoId), Unit.Value);

        _repositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
