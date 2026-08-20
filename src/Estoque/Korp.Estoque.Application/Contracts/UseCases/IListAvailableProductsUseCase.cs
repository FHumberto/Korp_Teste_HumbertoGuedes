using Korp.Estoque.Application.Features.Product.ListAvailableProducts;

namespace Korp.Estoque.Application.Contracts.UseCases;

public interface IListAvailableProductsUseCase
{
    Task<IReadOnlyCollection<ListAvailableProductsResponse>> ExecuteAsync(CancellationToken cancellationToken);
}
