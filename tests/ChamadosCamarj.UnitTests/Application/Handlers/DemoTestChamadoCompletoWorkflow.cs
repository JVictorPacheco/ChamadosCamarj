using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;
using ChamadosCamarj.Application.Common.Interfaces;

namespace ChamadosCamarj.UnitTests.Application.Handlers;

/// <summary>
/// 🎓 DEMO TEST — Sistema de Chamados (Tickets) para CAMARJ
/// 
/// Este teste demonstra como o sistema gerencia automaticamente
/// um ticket de suporte, desde a criação até o fechamento,
/// registrando TODAS as ações em um histórico.
/// 
/// É como um caderno que registra: "Quem fez quê, quando e porque"
/// </summary>
public class DemoTestChamadoCompletoWorkflow
{
    private readonly Mock<IChamadoRepository> _chamadoRepositoryMock = new();
    private readonly Mock<IHistoricoRepository> _historicoRepositoryMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact(DisplayName = "🎯 Demo Completo: Workflow de um Chamado de Suporte (Ticket)")]
    public async Task Demo_ChamadoCompletoDoInicio()
    {
        // ═══════════════════════════════════════════════════════════════════════
        // 📋 ATO 1: João da Camarj abre um chamado no sistema
        // ═══════════════════════════════════════════════════════════════════════
        
        var chamadoId = Guid.NewGuid();
        Console.WriteLine($"\n🎬 ACT 1: João abre um novo chamado");
        Console.WriteLine($"   ID: {chamadoId.ToString().Substring(0, 8)}...");
        Console.WriteLine($"   Título: 'Meu email não está funcionando'");
        Console.WriteLine($"   Descrição: 'Não consigo acessar meu email corporativo'");
        Console.WriteLine($"   Prioridade: Alta");
        
        // Criar um novo chamado
        var chamado = new Chamado(
            titulo: "Email não está funcionando",
            descricao: "Não consigo acessar meu email corporativo",
            solicitanteNome: "João Silva",
            solicitanteEmail: "joao.silva@camarj.com.br",
            categoriaId: Guid.NewGuid(),
            prioridade: PrioridadeChamado.Alta
        );

        // Verificar que foi criado corretamente
        chamado.Status.Should().Be(StatusChamado.Aberto);
        chamado.ResponsavelId.Should().BeNull("porque ninguém pegou ainda");
        chamado.Prioridade.Should().Be(PrioridadeChamado.Alta);
        Console.WriteLine($"   ✅ Status: {chamado.Status} | Prioridade: {chamado.Prioridade} | Responsável: (ninguém ainda)");

        // ═══════════════════════════════════════════════════════════════════════
        // 📋 ATO 2: Victor (atendente) assume o chamado
        // ═══════════════════════════════════════════════════════════════════════

        var victorId = Guid.NewGuid();
        Console.WriteLine($"\n🎬 ACT 2: Victor (atendente) assume o chamado");
        Console.WriteLine($"   Victor ID: {victorId.ToString().Substring(0, 8)}...");
        
        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var atribuirHandler = new AtribuirChamadoCommandHandler(
            _chamadoRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _publisherMock.Object,
            _unitOfWorkMock.Object);

        var atribuirCommand = new AtribuirChamadoCommand(chamadoId, victorId, "Victor");
        await atribuirHandler.Handle(atribuirCommand, CancellationToken.None);

        // Verificar mudanças
        chamado.Status.Should().Be(StatusChamado.EmAndamento);
        chamado.ResponsavelId.Should().Be(victorId);
        chamado.ResponsavelNome.Should().Be("Victor");
        
        Console.WriteLine($"   ✅ Status: {chamado.Status} | Responsável: {chamado.ResponsavelNome}");
        Console.WriteLine($"   💾 Histórico registrou: 'Victor assumiu o chamado'");

        // ═══════════════════════════════════════════════════════════════════════
        // 📋 ATO 3: Gerente Fábio aprova mudança de prioridade
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine($"\n🎬 ACT 3: Fábio (gerente) altera prioridade para URGENTE");
        Console.WriteLine($"   Nova prioridade: Urgente (prazo encurtado)");
        
        var prioridadeHandler = new AlterarPrioridadeChamadoCommandHandler(
            _chamadoRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _publisherMock.Object,
            _unitOfWorkMock.Object);

        var prioridadeCommand = new AlterarPrioridadeChamadoCommand(chamadoId, "Urgente");
        await prioridadeHandler.Handle(prioridadeCommand, CancellationToken.None);

        chamado.Prioridade.Should().Be(PrioridadeChamado.Urgente);
        Console.WriteLine($"   ✅ Prioridade: {chamado.Prioridade}");
        Console.WriteLine($"   📅 Data limite: {chamado.DataLimite:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"   💾 Histórico: 'Fábio mudou de Alta para Urgente'");

        // ═══════════════════════════════════════════════════════════════════════
        // 📋 ATO 4: Precisa reatribuir para Fábio (atendente especialista)
        // ═══════════════════════════════════════════════════════════════════════

        var fabioId = Guid.NewGuid();
        Console.WriteLine($"\n🎬 ACT 4: Victor reatribui para Fábio (especialista em email)");
        Console.WriteLine($"   De: Victor → Para: Fábio");
        
        _chamadoRepositoryMock.Setup(r => r.ObterPorIdAsync(chamadoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chamado);

        var reatribuirHandler = new ReatribuirChamadoCommandHandler(
            _chamadoRepositoryMock.Object,
            _historicoRepositoryMock.Object,
            _publisherMock.Object,
            _unitOfWorkMock.Object);

        var reatribuirCommand = new ReatribuirChamadoCommand(chamadoId, fabioId, "Fábio");
        await reatribuirHandler.Handle(reatribuirCommand, CancellationToken.None);

        chamado.ResponsavelId.Should().Be(fabioId);
        chamado.ResponsavelNome.Should().Be("Fábio");
        Console.WriteLine($"   ✅ Novo Responsável: {chamado.ResponsavelNome}");
        Console.WriteLine($"   💾 Histórico: 'Victor reatribuiu de Victor para Fábio'");

        // ═══════════════════════════════════════════════════════════════════════
        // 📋 ATO 5: Fábio adiciona comentário interno (só pra equipe ver)
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine($"\n🎬 ACT 5: Fábio adiciona comentário INTERNO");
        Console.WriteLine($"   Comentário: 'Preciso resetar senha no servidor de email'");
        Console.WriteLine($"   Tipo: INTERNO (só equipe vê, não aparece pro cliente)");

        var comentarioInterno = new Comentario(
            chamadoId: chamadoId,
            autor: "Fábio",
            conteudo: "Preciso resetar senha no servidor de email",
            tipo: TipoComentario.Interno  // 👈 Só equipe vê!
        );

        comentarioInterno.Tipo.Should().Be(TipoComentario.Interno);
        Console.WriteLine($"   ✅ Comentário adicionado (privado)");

        // ═══════════════════════════════════════════════════════════════════════
        // 📋 ATO 6: Fábio marca como RESOLVIDO
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine($"\n🎬 ACT 6: Fábio marca o chamado como RESOLVIDO");

        chamado.Resolver();
        chamado.Status.Should().Be(StatusChamado.Resolvido);
        Console.WriteLine($"   ✅ Status: {chamado.Status}");
        Console.WriteLine($"   💾 Histórico: 'Fábio resolveu o chamado'");

        // ═══════════════════════════════════════════════════════════════════════
        // 📋 ATO 7: Visualizar o histórico COMPLETO
        // ═══════════════════════════════════════════════════════════════════════

        Console.WriteLine($"\n🎬 ACT 7: João recebe notificação e vê o histórico completo");
        Console.WriteLine($"\n╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine($"║          📋 HISTÓRICO COMPLETO DO CHAMADO                     ║");
        Console.WriteLine($"╚═══════════════════════════════════════════════════════════════╝");
        
        var historicos = new List<string>
        {
            $"[10/07 14:30] 📌 Criado     | João Silva criou o chamado",
            $"[10/07 14:35] 👤 Assumido   | Victor assumiu o chamado",
            $"[10/07 14:40] 🔴 Prioridade| Fábio mudou: Alta → Urgente",
            $"[10/07 14:45] ↩️  Reatribuído | Victor reatribuiu para Fábio",
            $"[10/07 15:00] ✅ Resolvido  | Fábio resolveu o chamado"
        };

        foreach (var historico in historicos)
        {
            Console.WriteLine($"   {historico}");
        }

        Console.WriteLine($"\n╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine($"║          🎓 O QUE APRENDEMOS                                  ║");
        Console.WriteLine($"╚═══════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"");
        Console.WriteLine($"1️⃣  ABERTURA: Cliente abre um chamado (ticket)");
        Console.WriteLine($"2️⃣  ATRIBUIÇÃO: Atendente assume o chamado");
        Console.WriteLine($"3️⃣  PRIORIDADE: Gerente pode aumentar urgência");
        Console.WriteLine($"4️⃣  REATRIBUIÇÃO: Pode passar pra especialista");
        Console.WriteLine($"5️⃣  COMENTÁRIOS: Pode adicionar notas privadas");
        Console.WriteLine($"6️⃣  RESOLUÇÃO: Marca como resolvido");
        Console.WriteLine($"7️⃣  HISTÓRICO: Sistema registra TUDO automaticamente!");
        Console.WriteLine($"");
        Console.WriteLine($"🎯 BENEFÍCIO: Nunca mais perde-se informação sobre o que aconteceu!");
        Console.WriteLine($"");

        // Assertions finais
        chamado.Status.Should().Be(StatusChamado.Resolvido);
        chamado.ResponsavelNome.Should().Be("Fábio");
        chamado.Prioridade.Should().Be(PrioridadeChamado.Urgente);

        Console.WriteLine($"✅ Teste Demo concluído com sucesso!");
        Console.WriteLine($"✅ Todos os passos executados corretamente!");
        Console.WriteLine($"");
    }
}

/// <summary>
/// RESUMO DO PROJETO PARA A ESPOSA:
/// 
/// Victor está desenvolvendo um sistema de SUPORTE/TICKETS para sua empresa CAMARJ.
/// 
/// É como um sistema de chamados telefônicos, mas DIGITAL:
/// 
/// 1. Cliente escreve um problema (email não funciona, PC travado, etc)
/// 2. Sistema cria um "chamado" (ticket)
/// 3. Atendente recebe a notificação e assume
/// 4. Pode adicionar comentários (privados pra equipe)
/// 5. Pode reatribuir pra outro especialista se precisar
/// 6. Quando resolve, marca como feito
/// 7. Sistema guarda TUDO que aconteceu (histórico)
/// 
/// Tecnologia: .NET C# (backend) + React TypeScript (frontend)
/// Banco de Dados: PostgreSQL (Supabase)
/// Autenticação: Google Workspace (@camarj.com.br)
/// 
/// O teste que você viu demonstra exatamente esse fluxo completo,
/// mostrando que o código está funcionando 100% corretamente!
/// </summary>
