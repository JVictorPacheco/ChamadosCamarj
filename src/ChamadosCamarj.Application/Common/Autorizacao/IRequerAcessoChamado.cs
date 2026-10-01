using ChamadosCamarj.Domain.Enums;

namespace ChamadosCamarj.Application.Common.Autorizacao;

/// <summary>
/// Marca um command/query que lê ou altera um chamado específico. O AcessoChamadoBehaviour
/// checa visibilidade e permissão do usuário logado antes do handler rodar — todo request novo
/// sobre um chamado deve implementar esta interface.
/// </summary>
public interface IRequerAcessoChamado
{
    Guid ChamadoId { get; }
    AcaoChamado Acao { get; }
}
