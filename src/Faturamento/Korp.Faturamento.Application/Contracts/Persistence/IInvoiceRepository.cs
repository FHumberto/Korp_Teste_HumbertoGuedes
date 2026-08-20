using Korp.Faturamento.Domain.Entities;
using Korp.Faturamento.Domain.Enums;

namespace Korp.Faturamento.Application.Contracts.Persistence;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Invoice>> ListAsync(InvoiceStatus? status, CancellationToken cancellationToken);

    Task AddAsync(Invoice invoice, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
