using Korp.Faturamento.Application.Abstractions.Wrappers;
using Korp.Faturamento.Application.Contracts.Documents;
using Korp.Faturamento.Application.Contracts.Persistence;
using Korp.Faturamento.Application.Contracts.UseCases;
using Korp.Faturamento.Domain.Entities.Errors;
using Korp.Faturamento.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Korp.Faturamento.Application.Features.Invoice.GetInvoiceDocument;

public sealed class GetInvoiceDocumentHandler(
    IInvoiceRepository invoiceRepository,
    IInvoiceDocumentGenerator documentGenerator,
    ILogger<GetInvoiceDocumentHandler>? logger = null) : IGetInvoiceDocumentUseCase
{
    private readonly ILogger<GetInvoiceDocumentHandler> _logger = logger ?? NullLogger<GetInvoiceDocumentHandler>.Instance;

    public async Task<Result<GetInvoiceDocumentResponse>> ExecuteAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        Domain.Entities.Invoice? invoice = await invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);

        if (invoice is null)
            return Result<GetInvoiceDocumentResponse>.Failure(InvoiceErrors.NotFound);

        if (invoice.Status != InvoiceStatus.Closed)
            return Result<GetInvoiceDocumentResponse>.Failure(InvoiceErrors.NotClosed);

        try
        {
            _logger.LogInformation("Gerando PDF da nota {InvoiceId}, número {InvoiceNumber}.", invoice.Id, invoice.Number);
            byte[] content = documentGenerator.Generate(invoice);
            return Result<GetInvoiceDocumentResponse>.Success(new(content, $"nota-{invoice.Number:D6}.pdf"));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Falha ao gerar PDF da nota {InvoiceId}.", invoice.Id);
            return Result<GetInvoiceDocumentResponse>.Failure(GetInvoiceDocumentErrors.GenerationFailed);
        }
    }
}
