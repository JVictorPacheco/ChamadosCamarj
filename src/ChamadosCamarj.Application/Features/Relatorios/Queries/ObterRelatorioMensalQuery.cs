using MediatR;
using ChamadosCamarj.Application.Features.Relatorios.DTOs;

namespace ChamadosCamarj.Application.Features.Relatorios.Queries;

public record ObterRelatorioMensalQuery(int Ano, int Mes, Guid? ResponsavelId = null) : IRequest<RelatorioMensalResponse>;
