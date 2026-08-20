using Korp.Estoque.Application.Abstractions.Wrappers;
using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Features.Product.ListProducts;
using Korp.Estoque.Domain.Abstractions.Types;
using ProductEntity = Korp.Estoque.Domain.Entities.Product;

namespace Korp.Estoque.UnitTests.Features.Product;

public sealed class ListProductsHandlerTests
{
    [Fact]
    public async Task Execute_WithValidPagination_ShouldReturnMappedPage()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ProductEntity product = ProductEntity.Create(Guid.NewGuid(), "PROD-001", "Produto", 10, now);
        ProductRepositoryStub repository = new(new Paged<ProductEntity>([product], 21, 2, 10));
        ListProductsHandler handler = new(new ListProductsValidator(), repository);
        ListProductsRequest request = new() { PageNumber = 2, PageSize = 10 };

        Result<Paged<ListProductsResponse>> result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.PageNumber.ShouldBe(2);
        result.Value.PageSize.ShouldBe(10);
        result.Value.TotalRecords.ShouldBe(21);
        result.Value.TotalPages.ShouldBe(3);
        ListProductsResponse item = result.Value.Items.ShouldHaveSingleItem();
        item.Id.ShouldBe(product.Id);
        item.Code.ShouldBe(product.Code);
        item.Description.ShouldBe(product.Description);
        item.Balance.ShouldBe(product.Balance);
        repository.ReceivedPageNumber.ShouldBe(2);
        repository.ReceivedPageSize.ShouldBe(10);
    }

    [Fact]
    public async Task Execute_WithInvalidPagination_ShouldReturnValidationErrorBeforeQuery()
    {
        ProductRepositoryStub repository = new(new Paged<ProductEntity>([], 0, 1, 20));
        ListProductsHandler handler = new(new ListProductsValidator(), repository);
        ListProductsRequest request = new() { PageNumber = 0, PageSize = 101 };

        Result<Paged<ListProductsResponse>> result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe("VALIDATION_ERROR");
        result.Error.ValidationDetails.ShouldNotBeNull();
        result.Error.ValidationDetails.ShouldContainKey(nameof(ListProductsRequest.PageNumber));
        result.Error.ValidationDetails.ShouldContainKey(nameof(ListProductsRequest.PageSize));
        repository.QueryWasExecuted.ShouldBeFalse();
    }

    private sealed class ProductRepositoryStub(Paged<ProductEntity> page) : IProductRepository
    {
        public bool QueryWasExecuted { get; private set; }
        public int? ReceivedPageNumber { get; private set; }
        public int? ReceivedPageSize { get; private set; }

        public Task<Paged<ProductEntity>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            QueryWasExecuted = true;
            ReceivedPageNumber = pageNumber;
            ReceivedPageSize = pageSize;
            return Task.FromResult(page);
        }

        public Task<bool> TryAddAsync(ProductEntity product, CancellationToken cancellationToken) =>
            Task.FromResult(false);

        public Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<ProductEntity?>(null);

        public Task<IReadOnlyList<ProductEntity>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ProductEntity>>([]);
    }
}
