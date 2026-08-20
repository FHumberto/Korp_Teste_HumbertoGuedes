using Korp.Faturamento.Application.Features.Invoice.GetInvoice;

namespace Korp.Faturamento.Application.Contracts.UseCases;

public interface IGetInvoiceUseCase
{
    Task<Result<GetInvoiceResponse>> ExecuteAsync(Guid invoiceId, CancellationToken cancellationToken);
}
