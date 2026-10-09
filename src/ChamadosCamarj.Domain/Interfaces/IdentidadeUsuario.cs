using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Domain.Interfaces;

/// <summary>
/// O que o servidor confere no cadastro a cada pedido (spec perfil-no-cadastro): perfil, equipe e se a
/// conta está ativa. Projeção enxuta — sem o grafo do usuário.
/// </summary>
public record IdentidadeUsuario(Perfil Perfil, bool Ativo, Guid? GrupoId);
