using MediatR;
using ChamadosCamarj.Application.Features.Auth.DTOs;

namespace ChamadosCamarj.Application.Features.Auth.Commands;

public record AutenticarGoogleCommand(string IdToken) : IRequest<AutenticacaoResponse>;
