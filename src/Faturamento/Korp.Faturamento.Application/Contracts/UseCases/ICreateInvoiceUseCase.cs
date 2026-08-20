using Korp.Faturamento.Application.Abstractions.Wrappers;
using Korp.Faturamento.Application.Features.Invoice.CreateInvoice;

namespace Korp.Faturamento.Application.Contracts.UseCases;

public interface ICreateInvoiceUseCase
{
    Task<Result<CreateInvoiceResponse>> ExecuteAsync(CreateInvoiceRequest request, CancellationToken cancellationToken);
}
