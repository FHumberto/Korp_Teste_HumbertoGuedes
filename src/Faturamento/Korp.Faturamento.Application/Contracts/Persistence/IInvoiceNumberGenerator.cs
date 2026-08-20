namespace Korp.Faturamento.Application.Contracts.Persistence;

public interface IInvoiceNumberGenerator
{
    Task<long> GetNextAsync(CancellationToken cancellationToken);
}
