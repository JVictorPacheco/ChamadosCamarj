using ChamadosCamarj.Application.Common.Exceptions;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Common.Autorizacao;

/// <summary>
/// Regra única de quais módulos cada pessoa usa (spec controle-de-acesso). O cadastro guarda só as
/// exceções ao padrão do perfil (concedidos/retirados); por isso, sem ajuste, todo mundo fica igual ao
/// que era antes da feature (AC-16).
/// </summary>
public static class ModulosDeAcesso
{
    /// <summary>O que cada perfil tem sem nenhum ajuste — a matriz de antes da feature.</summary>
    public static ModuloSistema Padrao(Perfil perfil) => perfil switch
    {
        Perfil.Solicitante => ModuloSistema.Arquivo,
        _ => ModuloSistema.Todos
    };

    /// <summary>
    /// O que o Admin pode ligar/desligar para a pessoa. Solicitante só ganha módulos de consulta; Kanban e
    /// Fila são de atendimento (AC-08). Admin não é ajustável (AC-06).
    /// </summary>
    public static ModuloSistema Ajustaveis(Perfil perfil) => perfil switch
    {
        Perfil.Solicitante => ModuloSistema.Arquivo | ModuloSistema.Dashboard | ModuloSistema.RelatorioMensal,
        Perfil.Atendente => ModuloSistema.Todos,
        _ => ModuloSistema.Nenhum
    };

    /// <summary>
    /// Módulos que a pessoa de fato tem. Nunca passa do permitido para o perfil, mesmo que um ajuste
    /// inválido tenha sido gravado.
    /// </summary>
    public static ModuloSistema Efetivos(Perfil perfil, ModuloSistema concedidos, ModuloSistema retirados)
    {
        if (perfil == Perfil.Admin)
            return ModuloSistema.Todos;

        var permitidos = Padrao(perfil) | Ajustaveis(perfil);
        return ((Padrao(perfil) | concedidos) & ~retirados) & permitidos;
    }

    /// <summary>
    /// Converte os módulos que o Admin marcou em ajustes mínimos em relação ao padrão. Recusa módulo que o
    /// perfil não pode ajustar.
    /// </summary>
    public static (ModuloSistema Concedidos, ModuloSistema Retirados) CalcularAjustes(Perfil perfil, ModuloSistema desejados)
    {
        if (perfil == Perfil.Admin)
            throw new BadRequestException("O acesso do Admin é total e não pode ser ajustado.");

        var padrao = Padrao(perfil);
        var permitidos = padrao | Ajustaveis(perfil);
        if ((desejados & ~permitidos) != ModuloSistema.Nenhum)
            throw new BadRequestException("Este módulo não pode ser dado a este perfil.");

        return (desejados & ~padrao, padrao & ~desejados);
    }

    /// <summary>Nomes dos módulos, na ordem do menu — o formato que vai para a tela.</summary>
    public static IReadOnlyList<string> Nomes(ModuloSistema modulos) =>
        Enum.GetValues<ModuloSistema>()
            .Where(m => m is not (ModuloSistema.Nenhum or ModuloSistema.Todos) && modulos.HasFlag(m))
            .Select(m => m.ToString())
            .ToList();

    public static ModuloSistema DeNomes(IEnumerable<string> nomes)
    {
        var resultado = ModuloSistema.Nenhum;
        foreach (var nome in nomes)
        {
            if (!Enum.TryParse<ModuloSistema>(nome, ignoreCase: false, out var modulo)
                || modulo is ModuloSistema.Nenhum or ModuloSistema.Todos)
                throw new BadRequestException($"Módulo desconhecido: {nome}.");
            resultado |= modulo;
        }
        return resultado;
    }
}
