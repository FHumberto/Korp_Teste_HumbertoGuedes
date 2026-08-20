using Korp.Estoque.Application.Features.Product.GetProduct;

namespace Korp.Estoque.Application.Contracts.UseCases;

public interface IGetProductUseCase
{
    Task<Result<GetProductResponse>> ExecuteAsync(Guid id, CancellationToken cancellationToken);
}
