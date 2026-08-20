using Korp.Faturamento.Domain.Enums;

namespace Korp.Faturamento.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository(BillingDbContext dbContext) : IInvoiceRepository
{
    public Task<Invoice?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        return dbContext.Invoices
            .Include(invoice => invoice.Items)
            .SingleOrDefaultAsync(invoice => invoice.Id == invoiceId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Invoice>> ListAsync(InvoiceStatus? status, CancellationToken cancellationToken)
    {
        IQueryable<Invoice> query = dbContext.Invoices.AsNoTracking().Include(invoice => invoice.Items);

        if (status.HasValue)
            query = query.Where(invoice => invoice.Status == status.Value);

        return await query.OrderByDescending(invoice => invoice.Number).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
