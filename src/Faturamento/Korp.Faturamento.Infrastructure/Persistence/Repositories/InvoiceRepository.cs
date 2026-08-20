using Korp.Faturamento.Application.Contracts.Persistence;
using Korp.Faturamento.Domain.Entities;

namespace Korp.Faturamento.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository(BillingDbContext dbContext) : IInvoiceRepository
{
    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
