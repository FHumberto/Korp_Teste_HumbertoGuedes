using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Contracts.UseCases;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.Application.Features.Product.ListAvailableProducts;

public sealed class ListAvailableProductsHandler(IProductRepository productRepository, ILogger<ListAvailableProductsHandler>? logger = null) : IListAvailableProductsUseCase
{
    private readonly ILogger<ListAvailableProductsHandler> _logger = logger ?? NullLogger<ListAvailableProductsHandler>.Instance;

    public async Task<IReadOnlyCollection<ListAvailableProductsResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<ProductEntity> products = await productRepository.ListAvailableAsync(cancellationToken);
        List<ListAvailableProductsResponse> response = products
            .Select(product => new ListAvailableProductsResponse(product.Id, product.Code, product.Description, product.Balance))
            .ToList();
        _logger.LogInformation("Listagem de produtos disponíveis concluída com {ProductCount} itens.", response.Count);
        return response;
    }
}
