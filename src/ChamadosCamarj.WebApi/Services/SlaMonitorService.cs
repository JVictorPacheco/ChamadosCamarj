using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.WebApi.Services;

public class SlaMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SlaMonitorService> _logger;
    private readonly SlaAlertasEnviados _enviados = new();

    public SlaMonitorService(
        IServiceScopeFactory scopeFactory,
        ILogger<SlaMonitorService> logger)
    {
        _scopeFactory = scopeFactory;
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
            var notificador = scope.ServiceProvider.GetRequiredService<SlaAlertaNotificador>();

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
                var evento = _enviados.EventoANotificar(c.Id, status);
                if (evento is null) continue;

                var mensagem = evento == "SlaAtencao"
                    ? $"CAM-{c.Numero} — próximo do prazo!"
                    : $"CAM-{c.Numero} — PRAZO ESTOURADO!";
                _logger.LogInformation("{Evento}: CAM-{Numero}", evento, c.Numero);
                await notificador.NotificarAsync(c.Id, c.Numero, evento, mensagem, stoppingToken);
                _enviados.RegistrarEnvio(c.Id, status);
            }

            // Esquecer chamados que já foram finalizados
            _enviados.ManterSo(chamados.Select(c => c.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar SLA.");
        }
    }
}