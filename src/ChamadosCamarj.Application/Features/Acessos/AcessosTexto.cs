using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Features.Acessos;

/// <summary>Textos da auditoria de acessos, em português, como o Admin lê na tela (spec controle-de-acesso AC-14).</summary>
internal static class AcessosTexto
{
    public const string Ligado = "ligado";
    public const string Desligado = "desligado";

    public static string Modulo(ModuloSistema modulo) => modulo switch
    {
        ModuloSistema.RelatorioMensal => "Relatório mensal",
        _ => modulo.ToString()
    };

    public static string Chat(ChatPerfil perfil) => perfil switch
    {
        ChatPerfil.SemAcesso => "Sem acesso",
        ChatPerfil.Participante => "Participante",
        ChatPerfil.CriadorDeGrupo => "Pode criar grupos",
        _ => perfil.ToString()
    };

    /// <summary>Uma linha de auditoria por módulo que mudou entre "antes" e "depois".</summary>
    public static IEnumerable<AuditoriaAcesso> DiferencasDeModulos(
        UsuarioPerfil usuario, ModuloSistema antes, ModuloSistema depois, Guid adminId, string adminNome)
    {
        foreach (var modulo in Enum.GetValues<ModuloSistema>())
        {
            if (modulo is ModuloSistema.Nenhum or ModuloSistema.Todos) continue;
            var tinha = antes.HasFlag(modulo);
            var tem = depois.HasFlag(modulo);
            if (tinha == tem) continue;

            yield return AuditoriaAcesso.Criar(usuario.Id, usuario.Nome, adminId, adminNome,
                Modulo(modulo), tinha ? Ligado : Desligado, tem ? Ligado : Desligado);
        }
    }
}
