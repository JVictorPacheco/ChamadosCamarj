namespace ChamadosCamarj.Domain.Enums;

/// <summary>
/// Módulos ajustáveis por pessoa no controle de acesso (spec controle-de-acesso). "Abrir chamado",
/// "Meus chamados" e a Administração não entram: são fixos.
/// </summary>
[Flags]
public enum ModuloSistema
{
    Nenhum = 0,
    Arquivo = 1,
    Kanban = 2,
    Fila = 4,
    Dashboard = 8,
    RelatorioMensal = 16,
    Todos = Arquivo | Kanban | Fila | Dashboard | RelatorioMensal
}
