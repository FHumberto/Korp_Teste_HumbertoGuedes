using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Contracts.UseCases;
using Korp.Estoque.Domain.Entities.Errors;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.Application.Features.Product.GetProduct;

public sealed class GetProductHandler(IProductRepository productRepository, ILogger<GetProductHandler>? logger = null) : IGetProductUseCase
{
    private readonly ILogger<GetProductHandler> _logger = logger ?? NullLogger<GetProductHandler>.Instance;

    public async Task<Result<GetProductResponse>> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consultando produto {ProductId}.", id);
        ProductEntity? product = await productRepository.GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            _logger.LogWarning("Produto {ProductId} não encontrado.", id);
            return Result<GetProductResponse>.Failure(ProductErrors.NotFound);
        }

        _logger.LogInformation("Produto {ProductId} encontrado.", id);
        return Result<GetProductResponse>.Success(new GetProductResponse(product.Id, product.Code, product.Description, product.Balance, product.CreatedAt, product.UpdatedAt));
    }
}
