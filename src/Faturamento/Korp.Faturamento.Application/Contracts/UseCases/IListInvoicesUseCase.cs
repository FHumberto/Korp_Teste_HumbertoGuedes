using Korp.Faturamento.Application.Features.Invoice.ListInvoices;

namespace Korp.Faturamento.Application.Contracts.UseCases;

public interface IListInvoicesUseCase
{
    Task<Result<IReadOnlyCollection<ListInvoicesResponse>>> ExecuteAsync(ListInvoicesRequest request, CancellationToken cancellationToken);
}
