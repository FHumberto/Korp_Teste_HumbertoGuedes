using Korp.Estoque.Application.Features.Product.CreateProduct;

namespace Korp.Estoque.Application.Contracts.UseCases;

public interface ICreateProductUseCase
{
    Task<Result<CreateProductResponse>> ExecuteAsync(CreateProductRequest request, CancellationToken cancellationToken);
}
