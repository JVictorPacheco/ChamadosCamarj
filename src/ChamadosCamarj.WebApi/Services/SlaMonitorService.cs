using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Infrastructure.Data;
using ChamadosCamarj.WebApi.Hubs;

namespace ChamadosCamarj.WebApi.Services;

public class SlaMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<ChamadosHub> _hubContext;
    private readonly ILogger<SlaMonitorService> _logger;
    private readonly ConcurrentDictionary<Guid, SlaStatus> _notificados = new();

    public SlaMonitorService(
        IServiceScopeFactory scopeFactory,
        IHubContext<ChamadosHub> hubContext,
        ILogger<SlaMonitorService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SlaMonitorService iniciado.");
        while (!stoppingToken.IsCancellationRequested)
        {
            await VerificarSla(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task VerificarSla(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var chamados = await db.Chamados
                .AsNoTracking()
                .Where(c => c.Status != Domain.Enums.StatusChamado.Fechado
                         && c.Status != Domain.Enums.StatusChamado.Cancelado
                         && c.Status != Domain.Enums.StatusChamado.Resolvido
                         && c.DataLimite.HasValue)
                .Select(c => new { c.Id, c.Numero, c.Titulo, c.DataLimite })
                .ToListAsync(stoppingToken);

            foreach (var c in chamados)
            {
                var status = SlaCalculo.CalcularStatus(c.DataLimite);

                if (status == SlaStatus.Atencao && (!_notificados.ContainsKey(c.Id) || _notificados[c.Id] != SlaStatus.Atencao))
                {
                    _notificados[c.Id] = SlaStatus.Atencao;
                    _logger.LogInformation("SLA atenção: CAM-{Numero}", c.Numero);
                    await _hubContext.Clients.All.SendAsync("SlaAtencao", new
                    {
                        chamadoId = c.Id.ToString(),
                        numero = c.Numero,
                        titulo = c.Titulo,
                        mensagem = $"CAM-{c.Numero} — próximo do prazo!",
                    }, stoppingToken);
                }
                else if (status == SlaStatus.Atrasado && (!_notificados.ContainsKey(c.Id) || _notificados[c.Id] != SlaStatus.Atrasado))
                {
                    _notificados[c.Id] = SlaStatus.Atrasado;
                    _logger.LogInformation("SLA atrasado: CAM-{Numero}", c.Numero);
                    await _hubContext.Clients.All.SendAsync("SlaAtrasado", new
                    {
                        chamadoId = c.Id.ToString(),
                        numero = c.Numero,
                        titulo = c.Titulo,
                        mensagem = $"CAM-{c.Numero} — PRAZO ESTOURADO!",
                    }, stoppingToken);
                }
            }

            // Limpar chamados que já foram finalizados
            var finalizados = chamados.Select(c => c.Id).ToHashSet();
            foreach (var key in _notificados.Keys.Where(k => !finalizados.Contains(k)).ToList())
                _notificados.TryRemove(key, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar SLA.");
        }
    }
}