using Korp.Estoque.Application.Features.Product.ListProducts;

namespace Korp.Estoque.Application.Contracts.UseCases;

public interface IListProductsUseCase
{
    Task<Result<Paged<ListProductsResponse>>> ExecuteAsync(ListProductsRequest request, CancellationToken cancellationToken);
}
