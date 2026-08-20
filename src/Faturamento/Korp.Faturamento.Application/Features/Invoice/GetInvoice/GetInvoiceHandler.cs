using Korp.Faturamento.Application.Contracts.Persistence;
using Korp.Faturamento.Application.Contracts.UseCases;
using Korp.Faturamento.Domain.Entities.Errors;

namespace Korp.Faturamento.Application.Features.Invoice.GetInvoice;

public sealed class GetInvoiceHandler(IInvoiceRepository invoiceRepository, ILogger<GetInvoiceHandler>? logger = null) : IGetInvoiceUseCase
{
    private readonly ILogger<GetInvoiceHandler> _logger = logger ?? NullLogger<GetInvoiceHandler>.Instance;

    public async Task<Result<GetInvoiceResponse>> ExecuteAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consultando nota {InvoiceId}.", invoiceId);
        Domain.Entities.Invoice? invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null)
        {
            _logger.LogWarning("Nota {InvoiceId} não encontrada.", invoiceId);
            return Result<GetInvoiceResponse>.Failure(InvoiceErrors.NotFound);
        }

        _logger.LogInformation("Nota {InvoiceId} encontrada.", invoiceId);
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
