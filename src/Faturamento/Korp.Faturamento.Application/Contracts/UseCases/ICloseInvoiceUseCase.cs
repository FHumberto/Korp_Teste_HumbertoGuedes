using Korp.Faturamento.Application.Abstractions.Wrappers;
using Korp.Faturamento.Application.Features.Invoice.CloseInvoice;

namespace Korp.Faturamento.Application.Contracts.UseCases;

public interface ICloseInvoiceUseCase
{
    Task<Result<CloseInvoiceResponse>> ExecuteAsync(Guid invoiceId, CancellationToken cancellationToken);
}
