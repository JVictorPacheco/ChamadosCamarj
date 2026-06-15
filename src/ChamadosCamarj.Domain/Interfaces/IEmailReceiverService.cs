using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Domain.Interfaces;

public interface IEmailReceiverService
{
    Task ProcessarEmailsAsync(CancellationToken cancellationToken = default);
}
