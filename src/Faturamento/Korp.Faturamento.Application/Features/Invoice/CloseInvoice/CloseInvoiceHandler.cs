using Korp.Faturamento.Application.Contracts.Gateways;
using Korp.Faturamento.Application.Contracts.Persistence;
using Korp.Faturamento.Application.Contracts.UseCases;
using Korp.Faturamento.Domain.Entities.Errors;
using Korp.Faturamento.Domain.Enums;

namespace Korp.Faturamento.Application.Features.Invoice.CloseInvoice;

public sealed class CloseInvoiceHandler(
    IInvoiceRepository invoiceRepository,
    IInventoryGateway inventoryGateway,
    TimeProvider timeProvider) : ICloseInvoiceUseCase
{
    public async Task<Result<CloseInvoiceResponse>> ExecuteAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        Domain.Entities.Invoice? invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
            return Result<CloseInvoiceResponse>.Failure(InvoiceErrors.NotFound);

        if (invoice.Status != InvoiceStatus.Open)
            return Result<CloseInvoiceResponse>.Failure(InvoiceErrors.AlreadyClosed);

        DebitStockResult debitResult;
        try
        {
            debitResult = await inventoryGateway.DebitAsync(
                new DebitStockCommand(
                    invoice.Id,
                    invoice.Items.Select(item => new DebitStockItem(item.ProductId, item.Quantity)).ToArray()),
                $"invoice:{invoice.Id}:close:v1",
                cancellationToken);
        }
        catch (InventoryUnavailableException)
        {
            return Result<CloseInvoiceResponse>.Failure(CloseInvoiceErrors.InventoryUnavailable);
        }

        Error? error = debitResult.Status switch
        {
            DebitStockStatus.Succeeded => null,
            DebitStockStatus.ProductNotFound => CloseInvoiceErrors.ProductNotFound,
            DebitStockStatus.InsufficientStock => CloseInvoiceErrors.InsufficientStock,
            DebitStockStatus.IdempotencyConflict => CloseInvoiceErrors.IdempotencyConflict,
            _ => throw new InvalidOperationException($"Resultado de baixa não suportado: {debitResult.Status}.")
        };

        if (error is not null)
            return Result<CloseInvoiceResponse>.Failure(error);

        invoice.Close(timeProvider.GetUtcNow());
        await invoiceRepository.SaveChangesAsync(cancellationToken);

        return Result<CloseInvoiceResponse>.Success(new CloseInvoiceResponse(
            invoice.Id,
            invoice.Number,
            invoice.Status.ToString().ToLowerInvariant(),
            invoice.ClosedAt!.Value));
    }
}
