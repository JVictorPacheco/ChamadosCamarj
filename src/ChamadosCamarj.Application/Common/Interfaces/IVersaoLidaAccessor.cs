namespace ChamadosCamarj.Application.Common.Interfaces;

/// <summary>
/// Versão do chamado que o cliente leu antes de pedir a alteração (cabeçalho HTTP If-Match), ou
/// null se o cliente não informou — nesse caso não há checagem (spec correcoes-pre-deploy AC-23).
/// </summary>
public interface IVersaoLidaAccessor
{
    string? VersaoLida { get; }
}
