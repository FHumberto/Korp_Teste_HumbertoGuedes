using Korp.Estoque.Application.Abstractions.Wrappers;
using Korp.Estoque.Application.Features.Product.ListProducts;
using Korp.Estoque.Domain.Abstractions.Types;

namespace Korp.Estoque.Application.Contracts.UseCases;

public interface IListProductsUseCase
{
    Task<Result<Paged<ListProductsResponse>>> ExecuteAsync(ListProductsRequest request, CancellationToken cancellationToken);
}
