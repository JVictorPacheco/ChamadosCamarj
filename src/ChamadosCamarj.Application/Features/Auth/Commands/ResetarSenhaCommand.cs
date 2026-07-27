using MediatR;

namespace ChamadosCamarj.Application.Features.Auth.Commands;

public record ResetarSenhaCommand(string Token, string NovaSenha) : IRequest<bool>;
