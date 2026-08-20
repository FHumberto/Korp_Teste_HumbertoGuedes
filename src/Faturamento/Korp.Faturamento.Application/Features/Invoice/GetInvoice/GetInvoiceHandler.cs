using Korp.Faturamento.Application.Abstractions.Wrappers;
using Korp.Faturamento.Application.Contracts.Persistence;
using Korp.Faturamento.Application.Contracts.UseCases;
using Korp.Faturamento.Domain.Entities.Errors;

namespace Korp.Faturamento.Application.Features.Invoice.GetInvoice;

public sealed class GetInvoiceHandler(IInvoiceRepository invoiceRepository) : IGetInvoiceUseCase
{
    public async Task<Result<GetInvoiceResponse>> ExecuteAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        Domain.Entities.Invoice? invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
            return Result<GetInvoiceResponse>.Failure(InvoiceErrors.NotFound);

        return Result<GetInvoiceResponse>.Success(new GetInvoiceResponse(
            invoice.Id,
            invoice.Number,
            invoice.Status.ToString().ToLowerInvariant(),
            invoice.Items.Select(item => new GetInvoiceItemResponse(
                item.ProductId, item.ProductCode, item.ProductDescription, item.Quantity)).ToArray(),
            invoice.CreatedAt,
            invoice.ClosedAt));
    }
}
