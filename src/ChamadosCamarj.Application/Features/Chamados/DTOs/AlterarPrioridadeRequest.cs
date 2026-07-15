namespace ChamadosCamarj.Application.Features.Chamados.DTOs;

public record AlterarPrioridadeRequest(string NovaPrioridade, Guid? UsuarioId = null, string UsuarioNome = "Sistema");
