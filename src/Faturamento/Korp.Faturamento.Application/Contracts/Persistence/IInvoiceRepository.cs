using Korp.Faturamento.Domain.Entities;

namespace Korp.Faturamento.Application.Contracts.Persistence;

public interface IInvoiceRepository
{
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken);
}
