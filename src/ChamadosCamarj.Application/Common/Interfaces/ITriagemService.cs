namespace ChamadosCamarj.Application.Common.Interfaces;

public interface ITriagemService
{
    Task<TriagemSugestao> SugerirAsync(string titulo, string descricao, CancellationToken cancellationToken = default);
}
