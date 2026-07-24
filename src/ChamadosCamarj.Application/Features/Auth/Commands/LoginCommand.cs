using MediatR;
using ChamadosCamarj.Application.Features.Auth.DTOs;

namespace ChamadosCamarj.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Senha) : IRequest<AutenticacaoResponse>;
