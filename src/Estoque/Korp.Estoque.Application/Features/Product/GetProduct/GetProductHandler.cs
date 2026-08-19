using Korp.Estoque.Application.Abstractions.Wrappers;
using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Contracts.UseCases;
using Korp.Estoque.Domain.Entities.Errors;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.Application.Features.Product.GetProduct;

public sealed class GetProductHandler(IProductRepository productRepository) : IGetProductUseCase
{
    public async Task<Result<GetProductResponse>> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        ProductEntity? product = await productRepository.GetByIdAsync(id, cancellationToken);

        if (product is null)
            return Result<GetProductResponse>.Failure(ProductErrors.NotFound);

        return Result<GetProductResponse>.Success(new GetProductResponse(product.Id, product.Code, product.Description, product.Balance, product.CreatedAt, product.UpdatedAt));
    }
}
