using MediatR;

namespace ChamadosCamarj.Application.Features.Auth.Commands;

public record EsqueciSenhaCommand(string Email) : IRequest;
