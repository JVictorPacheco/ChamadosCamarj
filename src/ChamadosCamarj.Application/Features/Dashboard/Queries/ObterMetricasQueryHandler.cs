using MediatR;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Application.Features.Dashboard.DTOs;
using ChamadosCamarj.Domain.Enums;
using ChamadosCamarj.Domain.Interfaces;

namespace ChamadosCamarj.Application.Features.Dashboard.Queries;

public class ObterMetricasQueryHandler : IRequestHandler<ObterMetricasQuery, DashboardMetricsResponse>
{
    private readonly IChamadoRepository _chamadoRepository;
    private readonly ICurrentUserService _currentUser;

    public ObterMetricasQueryHandler(IChamadoRepository chamadoRepository, ICurrentUserService currentUser)
    {
        _chamadoRepository = chamadoRepository;
        _currentUser = currentUser;
    }

    public async Task<DashboardMetricsResponse> Handle(ObterMetricasQuery request, CancellationToken cancellationToken)
    {
        // Dashboard é só de Atendente/Admin; os números contam só o que o usuário pode ver (AC-13).
        var acesso = _currentUser.ObterContextoAcesso();
        if (acesso.Perfil == Perfil.Solicitante)
            throw new ForbiddenException("Você não tem permissão para acessar o dashboard.");

        var totalResolvidosHoje = await _chamadoRepository.ContarResolvidosHojeAsync(acesso, cancellationToken);
        var tempoMedio = await _chamadoRepository.ObterTempoMedioResolucaoHorasAsync(acesso, cancellationToken);
        var porCategoria = await _chamadoRepository.ContarPorCategoriaAsync(acesso, cancellationToken);
        var porPrioridade = await _chamadoRepository.ContarPorPrioridadeAsync(acesso, cancellationToken);

        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var fimMes = inicioMes.AddMonths(1).AddTicks(-1);
        var sla = await _chamadoRepository.ContarSlaComplianceAsync(acesso, inicioMes, fimMes, cancellationToken);

        SlaComplianceItem? slaItem = sla.TotalResolvidos > 0
            ? new SlaComplianceItem(sla.TotalResolvidos, sla.DentroPrazo, Math.Round((double)sla.DentroPrazo / sla.TotalResolvidos * 100, 1))
            : null;

        return new DashboardMetricsResponse(
            totalResolvidosHoje,
            tempoMedio.HasValue ? Math.Round(tempoMedio.Value, 1) : null,
            porCategoria.Select(c => new PorCategoriaItem(c.Nome, c.CategoriaId, c.Quantidade)).ToList(),
            porPrioridade.Select(kvp => new PorPrioridadeItem(kvp.Key, kvp.Value)).ToList(),
            slaItem
        );
    }
}
