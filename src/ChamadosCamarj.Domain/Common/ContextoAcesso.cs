using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Domain.Common;

/// <summary>
/// Quem está fazendo a requisição, sempre extraído do JWT — nunca de body ou query string.
/// É a única entrada das regras de visibilidade e de permissão de chamados.
/// </summary>
public sealed record ContextoAcesso(Guid UsuarioId, string Email, Perfil Perfil, Guid? GrupoId);
