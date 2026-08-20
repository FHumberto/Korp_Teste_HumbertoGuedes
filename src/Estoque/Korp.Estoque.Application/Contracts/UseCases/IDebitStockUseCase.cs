using Korp.Estoque.Application.Abstractions.Wrappers;
using Korp.Estoque.Application.Features.Stock.DebitStock;

namespace Korp.Estoque.Application.Contracts.UseCases;

public interface IDebitStockUseCase
{
    Task<Result<DebitStockResponse>> ExecuteAsync(
        DebitStockCommand command,
        CancellationToken cancellationToken);
}
