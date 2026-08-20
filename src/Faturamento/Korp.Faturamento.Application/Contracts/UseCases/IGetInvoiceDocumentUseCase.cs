using Korp.Faturamento.Application.Abstractions.Wrappers;
using Korp.Faturamento.Application.Features.Invoice.GetInvoiceDocument;

namespace Korp.Faturamento.Application.Contracts.UseCases;

public interface IGetInvoiceDocumentUseCase
{
    Task<Result<GetInvoiceDocumentResponse>> ExecuteAsync(Guid invoiceId, CancellationToken cancellationToken);
}
