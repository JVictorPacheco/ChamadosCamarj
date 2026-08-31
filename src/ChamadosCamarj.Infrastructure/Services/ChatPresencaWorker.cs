using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ChamadosCamarj.Application.Common.Notifications;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Infrastructure.Services;

/// <summary>
/// Roda a cada 60 segundos: marca usuários como Ausente após 5min sem heartbeat
/// e Offline após 15min. Broadcast das mudanças via ChatHub (por MediatR notification).
/// </summary>
public class ChatPresencaWorker : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan LimiteAusente = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan LimiteOffline = TimeSpan.FromMinutes(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ChatPresencaWorker> _logger;

    public ChatPresencaWorker(IServiceScopeFactory scopeFactory, ILogger<ChatPresencaWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ChatPresencaWorker iniciado.");
        while (!stoppingToken.IsCancellationRequested)
        {
            await VerificarPresencasAsync(stoppingToken);
            try
            {
                await Task.Delay(Intervalo, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private async Task VerificarPresencasAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IChatPresencaRepository>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var agora = DateTime.UtcNow;
            var alteradas = new List<Domain.Entities.ChatPresenca>();

            // Offline primeiro (limite maior) para não reprocessar como Ausente.
            var paraOffline = (await repo.ListarParaMarcarOfflineAsync(agora - LimiteOffline, cancellationToken)).ToList();
            foreach (var presenca in paraOffline)
            {
                presenca.MarcarOffline();
                alteradas.Add(presenca);
            }

            var idsOffline = paraOffline.Select(p => p.Id).ToHashSet();
            var paraAusente = (await repo.ListarParaMarcarAusenteAsync(agora - LimiteAusente, cancellationToken))
                .Where(p => !idsOffline.Contains(p.Id))
                .ToList();
            foreach (var presenca in paraAusente)
            {
                presenca.MarcarAusente();
                alteradas.Add(presenca);
            }

            if (alteradas.Count == 0)
                return;

            await repo.AtualizarVariasAsync(alteradas, cancellationToken);

            foreach (var presenca in alteradas)
            {
                await mediator.Publish(
                    new ChatPresencaAtualizadaNotification(presenca.UsuarioId, presenca.UsuarioNome, presenca.Status.ToString()),
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar presenças do chat.");
        }
    }
}
