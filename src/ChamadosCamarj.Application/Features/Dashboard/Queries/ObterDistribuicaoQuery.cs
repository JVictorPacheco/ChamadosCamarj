using MediatR;
using ChamadosCamarj.Application.Features.Dashboard.DTOs;

namespace ChamadosCamarj.Application.Features.Dashboard.Queries;

public record ObterDistribuicaoQuery : IRequest<DistribuicaoResponse>;
