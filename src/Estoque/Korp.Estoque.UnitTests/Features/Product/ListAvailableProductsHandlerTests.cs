using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Features.Product.ListAvailableProducts;
using Korp.Estoque.Domain.Abstractions.Types;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.UnitTests.Features.Product;

public sealed class ListAvailableProductsHandlerTests
{
    [Fact]
    public async Task Execute_WhenProductsAreAvailable_ShouldReturnRepositoryResults()
    {
        ProductEntity product = ProductEntity.Create(Guid.NewGuid(), "PROD-001", "Produto", 3, DateTimeOffset.UtcNow);
        var repository = new ProductRepositoryStub([product]);
        var handler = new ListAvailableProductsHandler(repository);

        IReadOnlyCollection<ListAvailableProductsResponse> result = await handler.ExecuteAsync(CancellationToken.None);

        ListAvailableProductsResponse item = result.ShouldHaveSingleItem();
        item.Id.ShouldBe(product.Id);
        item.Balance.ShouldBe(3);
        repository.QueryWasExecuted.ShouldBeTrue();
    }

    private sealed class ProductRepositoryStub(IReadOnlyList<ProductEntity> products) : IProductRepository
    {
        public bool QueryWasExecuted { get; private set; }
        public Task<IReadOnlyList<ProductEntity>> ListAvailableAsync(CancellationToken cancellationToken) { QueryWasExecuted = true; return Task.FromResult(products); }
        public Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<ProductEntity?>(null);
        public Task<IReadOnlyList<ProductEntity>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ProductEntity>>([]);
        public Task<Paged<ProductEntity>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken) => Task.FromResult(new Paged<ProductEntity>([], 0, pageNumber, pageSize));
        public Task<bool> TryAddAsync(ProductEntity product, CancellationToken cancellationToken) => Task.FromResult(false);
    }
}
