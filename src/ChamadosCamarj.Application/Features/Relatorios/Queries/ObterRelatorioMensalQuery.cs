using MediatR;
using ChamadosCamarj.Application.Features.Relatorios.DTOs;

namespace ChamadosCamarj.Application.Features.Relatorios.Queries;

/// <param name="IdsVisiveis">Quando informado, só conta estes chamados (Solicitante com o módulo — spec
/// controle-de-acesso AC-07). Null = sem esse filtro.</param>
public record ObterRelatorioMensalQuery(int Ano, int Mes, Guid? ResponsavelId = null, IReadOnlyCollection<Guid>? IdsVisiveis = null) : IRequest<RelatorioMensalResponse>;
